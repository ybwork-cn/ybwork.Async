// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Generic;
using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    internal class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }
    }

    internal class TaskManager : MonoSingleton<TaskManager>
    {
        public int Count;
        private readonly List<AwaiterBase> _taskAwaiters1 = new();
        private readonly List<AwaiterBase> _taskAwaiters2 = new();

        public void AddTaskAwaiter(AwaiterBase taskAwaiter)
        {
            _taskAwaiters2.Add(taskAwaiter);
        }

        private void Update()
        {
            _taskAwaiters1.Clear();
            _taskAwaiters1.AddRange(_taskAwaiters2);
            _taskAwaiters2.Clear();

            foreach (AwaiterBase awaiter in _taskAwaiters1)
            {
                if (awaiter.IsCompleted)
                    continue;

                try
                {
                    if (awaiter.MoveNext())
                        _taskAwaiters2.Add(awaiter);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Count = _taskAwaiters1.Count;
        }
    }
}
