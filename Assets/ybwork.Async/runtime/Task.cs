// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "YBT011:异步方法名应有Async后缀", Justification = "<挂起>")]
    [AsyncMethodBuilder(typeof(YueTaskMethodBuilder))]  //允许YueTask<>作为异步函数的返回值
    public class YueTask
    {
        private protected readonly IAwaiter _awaiter;

        public YueTask()
        {
            _awaiter = new Awaiter();
        }

        private protected YueTask(AwaiterBase awaiter)
        {
            _awaiter = awaiter;
        }

        public IAwaiterVoid GetAwaiter()
        {
            if (_awaiter is not IAwaiterVoid awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiterVoid).Name}");

            return awaiter.State switch
            {
                AwaiterState.Aborted => awaiter,
                AwaiterState.Error => awaiter,
                AwaiterState.Started => awaiter,
                AwaiterState.Completed => awaiter,
                _ => throw new NotImplementedException(),
            };
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue()
        {
            if (_awaiter is not IAwaiterVoid awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiterVoid).Name}");

            awaiter.SetValue();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetException()
        {
            _awaiter.SetException();
        }

        public void Cancel()
        {
            _awaiter.Cancel();
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="action"></param>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Then(Action action)
        {
            _awaiter.OnCompleted(action);
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="source"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask ContinueWith(Func<YueTask> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            await this;
            await func.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<YueTask<TResult>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            await this;
            return await func.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<TResult> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            await this;
            return func.Invoke();
        }

        public static YueTask CompletedTask = new YueTask(new CompletedAwaiter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask<T> FromResult<T>(T value)
        {
            YueTask<T> task = new YueTask<T>();
            task.SetValue(value);
            return task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask WaitAny(params YueTask[] tasks)
        {
            YueTask result = new YueTask(new MutiAwaiter(tasks, MutiAwaiter.WaiteType.WaitAny));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask WaitAll(params YueTask[] tasks)
        {
            return WaitAll((IReadOnlyCollection<YueTask>)tasks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask WaitAll(IReadOnlyCollection<YueTask> tasks)
        {
            YueTask result = new YueTask(new MutiAwaiter(tasks, MutiAwaiter.WaiteType.WaitAll));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask<T[]> WaitAll<T>(params YueTask<T>[] tasks)
        {
            return WaitAll((IReadOnlyCollection<YueTask<T>>)tasks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YueTask<T[]> WaitAll<T>(IReadOnlyCollection<YueTask<T>> tasks)
        {
            YueTask<T[]> result = new YueTask<T[]>(new MutiAwaiter<T>(tasks, MutiAwaiter.WaiteType.WaitAll));
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
        public static YueTask WaitToMainThread() => YueTaskManager.IsMainThread ? CompletedTask : Yield();

        public static YueTask Delay(float seconds) => new YueTask(new DeleyAwaiter(seconds));
        public static YueTask DelayFrames(int frameCount) => frameCount > 0
            ? new YueTask(new DeleyFramesAwaiter(frameCount))
            : CompletedTask;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "YBT011:异步方法名应有Async后缀", Justification = "<挂起>")]
    [AsyncMethodBuilder(typeof(YueTaskMethodBuilder<>))]  //允许ybwork.Async.Task<>作为异步函数的返回值
    public class YueTask<T> : YueTask
    {
        public YueTask() : base(new Awaiter<T>())
        {
        }

        internal YueTask(Awaiter<T> awaiter) : base(awaiter)
        {
        }

        public new IAwaiter<T> GetAwaiter()
        {
            if (_awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            return awaiter.State switch
            {
                AwaiterState.Aborted => awaiter,
                AwaiterState.Error => awaiter,
                AwaiterState.Started => awaiter,
                AwaiterState.Completed => awaiter,
                _ => throw new NotImplementedException(),
            };
        }

        [Obsolete]
        public new void SetValue()
        {
            base.SetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            if (_awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            ((IAwaiter<T>)awaiter).SetValue(result);
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="action"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Then(Action<T> action)
        {
            if (_awaiter is not Awaiter<T> awaiter)
                throw new InvalidCastException($"当前YueTask的Awaiter不为{typeof(IAwaiter<T>).Name}");

            awaiter.OnCompleted(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask ContinueWith(Func<T, YueTask> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            await func.Invoke(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask ContinueWith(Action<T> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            func.Invoke(value);
            return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<T, YueTask<TResult>> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            return await func.Invoke(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async YueTask<TResult> ContinueWith<TResult>(Func<T, TResult> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));

            T value = await this;
            return func.Invoke(value);
        }

        public T GetResult() => GetAwaiter().GetResult();
    }
}
