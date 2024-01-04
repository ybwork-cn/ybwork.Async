// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Runtime.CompilerServices;

namespace ybwork.Async
{
    public class TaskMethodBuilder
    {
        public static TaskMethodBuilder Create() => new TaskMethodBuilder();

        public YueTask Task => _response;
        private readonly YueTask _response;

        public TaskMethodBuilder() { _response = new YueTask(); }

        public void SetResult(object result) => _response.SetValue(result);
        public void SetResult() => _response.SetValue(null);
        public void SetException(Exception ex) => _response.SetException(ex);

        public void SetStateMachine(IAsyncStateMachine _) { }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.OnCompleted(stateMachine.MoveNext);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }

    public class TaskMethodBuilder<T>
    {
        public static TaskMethodBuilder<T> Create() => new TaskMethodBuilder<T>();

        public YueTask<T> Task => _response;
        private readonly YueTask<T> _response;

        public TaskMethodBuilder() { _response = new YueTask<T>(); }

        public void SetResult(T result) => _response.SetValue(result);
        public void SetException(Exception ex) => _response.SetException(ex);

        public void SetStateMachine(IAsyncStateMachine _) { }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.OnCompleted(stateMachine.MoveNext);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }
}
