// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    public class YueTaskMethodBuilder
    {
        public static YueTaskMethodBuilder Create() => new YueTaskMethodBuilder();

        public YueTask Task => _response;
        private readonly YueTask _response;

        public YueTaskMethodBuilder()
        {
            _response = new YueTask();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult() => _response.SetValue();

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception ex)
        {
            _response.SetException();
            UnityEngine.Debug.LogException(ex);
        }

        public void SetStateMachine(IAsyncStateMachine _) { }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Action action = stateMachine.MoveNext;
            awaiter.OnCompleted(() =>
            {
                if (_response.GetAwaiter().State is AwaiterState.Started or AwaiterState.Completed)
                    action?.Invoke();
            });
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }

    public class YueTaskMethodBuilder<T>
    {
        public static YueTaskMethodBuilder<T> Create() => new YueTaskMethodBuilder<T>();

        public YueTask<T> Task => _response;
        private readonly YueTask<T> _response;

        public YueTaskMethodBuilder() { _response = new YueTask<T>(); }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T result) => _response.SetValue(result);

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception ex)
        {
            _response.SetException();
            UnityEngine.Debug.LogException(ex);
        }

        public void SetStateMachine(IAsyncStateMachine _) { }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Action action = stateMachine.MoveNext;
            awaiter.OnCompleted(() =>
            {
                if (_response.GetAwaiter().State is AwaiterState.Started or AwaiterState.Completed)
                    action?.Invoke();
            });
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
}
