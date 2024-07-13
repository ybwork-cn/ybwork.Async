// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    [AsyncMethodBuilder(typeof(TaskMethodBuilder))]  //允许YueTask<>作为异步函数的返回值
    public class YueTask
    {
        internal IAwaiter Awaiter;

        public YueTask()
        {
            Awaiter = new Awaiter();
        }

        private protected YueTask(AwaiterBase awaiter)
        {
            Awaiter = awaiter;
        }

        public IAwaiterVoid GetAwaiter()
        {
            if (Awaiter is not IAwaiterVoid awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiterVoid).Name}");

            return awaiter.State switch
            {
                AwaiterState.Aborted => throw new InvalidOperationException("不可调用已取消的YueTask的GetAwaiter()"),
                AwaiterState.Error => throw new InvalidOperationException("不可调用已抛出错误的YueTask的GetAwaiter()"),
                AwaiterState.Started => awaiter,
                AwaiterState.Completed => awaiter,
                _ => throw new NotImplementedException(),
            };
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue()
        {
            if (Awaiter is not IAwaiterVoid awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiterVoid).Name}");

            awaiter.SetValue();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetException()
        {
            Awaiter.SetException();
        }

        public void Cancel()
        {
            Awaiter.Cancel();
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="action"></param>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Then(Action action)
        {
            Awaiter.OnCompleted(action);
        }

        public static YueTask CompletedTask = new YueTask(new CompletedAwaiter());

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask WaitAny(params YueTask[] tasks)
        {
            YueTask result = new YueTask(new MutiAwaiter(tasks, MutiAwaiter.WaiteType.WaitAny));
            return result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask WaitAll(params YueTask[] tasks)
        {
            YueTask result = new YueTask(new MutiAwaiter(tasks, MutiAwaiter.WaiteType.WaitAll));
            return result;
        }

        public static YueTask Delay(float seconds) => new YueTask(new DeleyAwaiter(seconds));
        public static YueTask DelayFrames(int frameCount) => new YueTask(new DeleyFramesAwaiter(frameCount));
        public static YueTask Yield() => new YueTask(new YieldAwaiter());
        public static YueTask WaitUntil(Func<bool> predicate) => new YueTask(new WaitUntilAwater(predicate));
    }

    [AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]  //允许ybwork.Async.Task<>作为异步函数的返回值
    public class YueTask<T> : YueTask
    {
        public YueTask() : base(new Awaiter<T>())
        {
        }

        public new IAwaiter<T> GetAwaiter()
        {
            if (Awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            return awaiter.State switch
            {
                AwaiterState.Aborted => throw new InvalidOperationException("不可调用已取消的YueTask的GetAwaiter()"),
                AwaiterState.Error => throw new InvalidOperationException("不可调用已抛出错误的YueTask的GetAwaiter()"),
                AwaiterState.Started => awaiter,
                AwaiterState.Completed => awaiter,
                _ => throw new NotImplementedException(),
            };
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete]
        public new void SetValue()
        {
            base.SetValue();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            if (Awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            awaiter.SetValue(result);
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="action"></param>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Then(Action<T> action)
        {
            if (Awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            awaiter.OnCompleted(action);
        }
    }
}
