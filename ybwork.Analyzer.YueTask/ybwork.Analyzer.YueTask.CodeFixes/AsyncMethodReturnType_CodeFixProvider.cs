using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ybwork.Analyzer.YueTask
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncMethodReturnType_Analyzer)), Shared]
    public class AsyncMethodReturnType_CodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncMethodReturnType_Analyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            MethodDeclarationSyntax declaration = root.FindNode(diagnosticSpan) as MethodDeclarationSyntax;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: AsyncMethodReturnType_Analyzer.Title,
                    createChangedDocument: c => ReplaceTaskMethodNameAsync(context, declaration, c),
                    equivalenceKey: nameof(AsyncMethodReturnType_Analyzer.Title)),
                diagnostic);
        }

        private async Task<Document> ReplaceTaskMethodNameAsync(CodeFixContext context, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
        {
            Document document = context.Document;
            string typeName = methodDecl.ReturnType.ToFullString();
            string genericTypeName = Regex.Match(typeName, @"<(.+?)>").Groups[1].Value;

            typeName = string.IsNullOrEmpty(genericTypeName)
                ? "YueTask"
                : $"YueTask<{genericTypeName}>";
            methodDecl.ReturnType.ToFullString();

            // 创建新的返回类型
            TypeSyntax newReturnType = SyntaxFactory.ParseTypeName(typeName).WithTriviaFrom(methodDecl.ReturnType);

            // 替换旧的返回类型
            MethodDeclarationSyntax newMethodDecl = methodDecl.WithReturnType(newReturnType);

            // 获取语法树的根节点并替换方法声明
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = root.ReplaceNode(methodDecl, newMethodDecl);

            // 返回更新后的文档
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
