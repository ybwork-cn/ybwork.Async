// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Generic;
using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    public sealed class YueTaskManager : MonoBehaviour
    {
        private static YueTaskManager _instance;
        public static YueTaskManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject(nameof(YueTaskManager)).AddComponent<YueTaskManager>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        public int Count;
        private readonly List<IAwaiter> _taskAwaiters1 = new();
        private readonly List<IAwaiter> _taskAwaiters2 = new();

        public void AddTaskAwaiter(IAwaiter taskAwaiter)
        {
            _taskAwaiters2.Add(taskAwaiter);
        }

        /// <summary>
        /// 停止所有正在运行的YueTask
        /// </summary>
        public void CancelAllTask()
        {
            foreach (IAwaiter awaiter in _taskAwaiters1)
            {
                if (awaiter.State == AwaiterState.Started)
                    awaiter.Cancel();
            }
            foreach (IAwaiter awaiter in _taskAwaiters2)
            {
                if (awaiter.State == AwaiterState.Started)
                    awaiter.Cancel();
            }
        }

        private void Update()
        {
            _taskAwaiters1.Clear();
            _taskAwaiters1.AddRange(_taskAwaiters2);
            _taskAwaiters2.Clear();

            foreach (IAwaiter awaiter in _taskAwaiters1)
            {
                if (awaiter.State != AwaiterState.Started)
                    continue;

                try
                {
                    awaiter.MoveNext();
                    if (awaiter.State == AwaiterState.Started)
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
