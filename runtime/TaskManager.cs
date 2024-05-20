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
        private readonly List<AwaiterBase> _taskAwaiters = new();

        public void AddTaskAwaiter(AwaiterBase taskAwaiter)
        {
            _taskAwaiters.Add(taskAwaiter);
        }

        private void Update()
        {
            for (int i = 0; i < _taskAwaiters.Count; i++)
            {
                AwaiterBase awaiter = _taskAwaiters[i];

                bool remove = false;
                try
                {
                    if (awaiter.IsCompleted || !awaiter.MoveNext())
                    {
                        awaiter.Complete();
                        remove = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    remove = true;
                }
                finally
                {
                    if (remove)
                    {
                        _taskAwaiters.RemoveAt(i);
                        i--;
                    }
                }
            }
            Count = _taskAwaiters.Count;
        }
    }
}
