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
        protected readonly AwaiterBase TaskAwaiter;

        public YueTask()
        {
            TaskAwaiter = new AwaiterBase();
        }

        protected YueTask(AwaiterBase awaiter)
        {
            TaskAwaiter = awaiter;
        }

        public AwaiterBase GetAwaiter()
        {
            return TaskAwaiter;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(object result)
        {
            TaskAwaiter.SetValue(result);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException()
        {
            TaskAwaiter.SetException();
        }

        /// <summary>
        /// 多次注册不保证触发顺序固定
        /// </summary>
        /// <param name="action"></param>
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Then(Action action)
        {
            TaskAwaiter.OnCompleted(action);
        }

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
        private new Awaiter<T> TaskAwaiter => base.TaskAwaiter as Awaiter<T>;

        public YueTask() : base(new Awaiter<T>())
        {
        }

        public new Awaiter<T> GetAwaiter()
        {
            return TaskAwaiter;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            Awaiter<T> awaiter = TaskAwaiter;
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
            TaskAwaiter.OnCompleted(action);
        }
    }
}
