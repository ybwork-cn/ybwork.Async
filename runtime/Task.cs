// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections;
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
            TaskAwaiter = new Awaiter();
        }

        protected YueTask(AwaiterBase awaiter)
        {
            TaskAwaiter = awaiter;
        }

        public AwaiterBase GetAwaiter()
        {
            return TaskAwaiter;
        }

        public void SetValue(object result)
        {
            TaskAwaiter.SetValue(result);
        }

        public void SetException()
        {
            TaskAwaiter.SetException();
        }

        public void Then(Action action)
        {
            TaskAwaiter.OnCompleted(action);
        }

        public static YueTask WaitAny(params YueTask[] tasks)
        {
            bool waiting = true;
            YueTask result = new YueTask();
            foreach (var task in tasks)
            {
                task.Then(() =>
                {
                    if (!waiting)
                        return;
                    waiting = false;
                    result.SetValue(null);
                });
            }
            return result;
        }

        public static YueTask WaitAll(params YueTask[] tasks)
        {
            YueTask result = new YueTask();
            int count = tasks.Length;
            foreach (var task in tasks)
            {
                task.Then(() =>
                {
                    count--;
                    if (count == 0)
                        result.SetValue(null);
                });
            }
            return result;
        }

        public static YueTask Delay(float seconds) => new YueTask(new DeleyAwaiter(seconds));
        public static YueTask DelayFrame(int frameCount) => new YueTask(new Awaiter(DelayFrameFunc(frameCount)));
        public static YueTask Yield() => new YueTask(new Awaiter(YieldFnuc()));
        public static YueTask WaitUntil(Func<bool> predicate) => new YueTask(new Awaiter(WaitUntilFunc(predicate)));
        private static IEnumerator DelayFrameFunc(int frameCount)
        {
            while (frameCount > 0)
            {
                yield return frameCount;
                frameCount--;
            }
        }
        private static IEnumerator WaitUntilFunc(Func<bool> predicate)
        {
            while (!predicate.Invoke())
                yield return 0;
        }
        private static IEnumerator YieldFnuc() { yield return 0; }
    }

    [AsyncMethodBuilder(typeof(TaskMethodBuilder<>))]  //允许ybwork.Async.Task<>作为异步函数的返回值
    public class YueTask<T> : YueTask
    {
        public YueTask() : base(new Awaiter<T>())
        {
        }

        public new Awaiter<T> GetAwaiter()
        {
            return TaskAwaiter as Awaiter<T>;
        }

        public void SetValue(T result)
        {
            base.SetValue(result);
        }

        public void Then(Action<T> action)
        {
            Then(() =>
            {
                T result = (TaskAwaiter as Awaiter<T>).GetResult();
                action?.Invoke(result);
            });
        }
    }
}
