// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System.Runtime.CompilerServices;

namespace ybwork.Async.Awaiters
{
    public interface IAwaiter : INotifyCompletion
    {
        AwaiterState State { get; }
        bool IsCompleted { get; }
        internal void MoveNext();
        internal void SetException();
        void Cancel();
    }

    public interface IAwaiterVoid : IAwaiter
    {
        void GetResult();
        internal void SetValue();
    }

    public interface IAwaiter<T> : IAwaiter
    {
        T GetResult();
    }
}
