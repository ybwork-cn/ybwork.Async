// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
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
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void GetResult()
        {
            switch (State)
            {
                case AwaiterState.Started:
                    throw new InvalidOperationException("YueTask未完成");
                case AwaiterState.Completed:
                    break;
                case AwaiterState.Aborted:
                    throw new InvalidOperationException("YueTask已取消");
                case AwaiterState.Error:
                    throw new InvalidOperationException("YueTask已发生错误");
                default:
                    throw new NotImplementedException();
            }
        }
        internal void SetValue();
    }

    public interface IAwaiter<T> : IAwaiter
    {
        T GetResult();
        internal void SetValue(T result);
    }
}
