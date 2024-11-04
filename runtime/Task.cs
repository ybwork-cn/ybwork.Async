// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    [AsyncMethodBuilder(typeof(YueTaskMethodBuilder))]  //允许YueTask<>作为异步函数的返回值
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

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="source"></param>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask ContinueWith(Func<YueTask> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            await this;
            await func.Invoke();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<YueTask<TResult>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            await this;
            return await func.Invoke();
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

        /// <summary>
        /// 等待到下一个渲染帧执行
        /// </summary>
        /// <returns></returns>
        public static YueTask Yield() => new YueTask(new YieldAwaiter());

        /// <summary>
        /// 等待到主线程执行（如果当前在主线程中，立即执行）
        /// </summary>
        /// <returns></returns>
        public static YueTask WaitToMainThread() => YueTaskManager.Instance.IsMainThread ? CompletedTask : Yield();

        public static YueTask Delay(float seconds) => new YueTask(new DeleyAwaiter(seconds));
        public static YueTask DelayFrames(int frameCount) => new YueTask(new DeleyFramesAwaiter(frameCount));
        public static YueTask WaitUntil(Func<bool> predicate) => new YueTask(new WaitUntilAwater(predicate));
        public static YueTask Run(Func<YueTask> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return func.Invoke();
        }
        public static YueTask<T> Run<T>(Func<YueTask<T>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return func.Invoke();
        }
    }

    [AsyncMethodBuilder(typeof(YueTaskMethodBuilder<>))]  //允许ybwork.Async.Task<>作为异步函数的返回值
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

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<T> ContinueWith(Func<T, YueTask<T>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            T result = await func.Invoke(value);
            return result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask ContinueWith(Func<T, YueTask> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            await func.Invoke(value);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<T, YueTask<TResult>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            return await func.Invoke(value);
        }
    }
}
