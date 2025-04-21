using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ybwork.Analyzer.YueTask
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodReturnType_Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "YBT012";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        public const string Title = "返回值应改为YueTask";
        private const string MessageFormat = "异步方法 '{0}' 返回值应改为YueTask";
        private const string Description = "返回值应改为YueTask.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax methodDeclaration = context.Node as MethodDeclarationSyntax;
            IMethodSymbol methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (!IsTypeTask(context.SemanticModel, methodSymbol.ReturnType))
                return;

            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                methodDeclaration.GetLocation(),
                methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsTypeTask(SemanticModel semanticModel, ITypeSymbol typeSymbol)
        {
            // 获取 YueTask 类型
            INamedTypeSymbol taskType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            if (taskType == null)
                return false;

            // 检查是否是 YueTask 或者 YueTask<T>
            return SymbolEqualityComparer.Default.Equals(typeSymbol, taskType)
                || typeSymbol is INamedTypeSymbol namedType
                    && namedType.IsGenericType
                    && SymbolEqualityComparer.Default.Equals(namedType.BaseType, taskType);
        }
    }
}
