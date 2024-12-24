// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    public static class AwaiterExtension
    {
        public static IAwaiterVoid GetAwaiter(this AsyncOperation operation)
        {
            YueTask task = new YueTask();
            operation.completed += (_) => task.SetValue();
            return task.GetAwaiter();
        }
    }
}
