using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    public static class AwaiterExtension
    {
        public static AwaiterBase GetAwaiter(this AsyncOperation operation)
        {
            YueTask task = YueTask.WaitUntil(() => operation.isDone);
            return task.GetAwaiter();
        }
    }
}
