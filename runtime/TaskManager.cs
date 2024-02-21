// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System.Collections.Generic;
using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }
        public int Count;
        private readonly List<AwaiterBase> TaskAwaiters = new();

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            Instance = new GameObject(nameof(TaskManager)).AddComponent<TaskManager>();
            DontDestroyOnLoad(Instance);
        }

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
                    TaskAwaiters.RemoveAt(i);
                    i--;
                }
            }
            Count = TaskAwaiters.Count;
        }
    }
}
