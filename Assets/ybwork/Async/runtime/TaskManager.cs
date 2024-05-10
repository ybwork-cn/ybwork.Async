// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System.Collections.Generic;
using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    class TaskManager : MonoBehaviour
    {
        private static TaskManager instance;
        public static TaskManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(nameof(TaskManager)).AddComponent<TaskManager>();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }
        public int Count;
        private readonly List<AwaiterBase> TaskAwaiters = new();

        public void AddTaskAwaiter(AwaiterBase taskAwaiter)
        {
            TaskAwaiters.Add(taskAwaiter);
        }

        private void Update()
        {
            for (int i = 0; i < TaskAwaiters.Count; i++)
            {
                AwaiterBase awaiter = TaskAwaiters[i];
                if (awaiter.IsCompleted || !awaiter.MoveNext())
                {
                    awaiter.Complete();
                    TaskAwaiters.RemoveAt(i);
                    i--;
                }
            }
            Count = TaskAwaiters.Count;
        }
    }
}
