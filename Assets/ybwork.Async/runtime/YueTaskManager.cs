// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Concurrent;
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
                    Init();
                return _instance;
            }
        }

        public static void Init()
        {
            if (Application.isEditor && !Application.isPlaying)
                throw new InvalidOperationException($"禁止在编辑器退出后调用{nameof(YueTaskManager)}");
            if (_instance == null)
                _instance = FindObjectOfType<YueTaskManager>();
            if (_instance == null)
            {
                _instance = new GameObject(nameof(YueTaskManager)).AddComponent<YueTaskManager>();
                DontDestroyOnLoad(_instance);
            }
        }

        public int Count;
        private readonly List<IAwaiter> _awaiters = new();
        private readonly ConcurrentQueue<IAwaiter> _testAwaiters = new();

        public void AddTaskAwaiter(IAwaiter taskAwaiter)
        {
            _testAwaiters.Enqueue(taskAwaiter);
        }

        /// <summary>
        /// 停止所有正在运行的YueTask
        /// </summary>
        public void CancelAllTask()
        {
            foreach (IAwaiter awaiter in _awaiters)
            {
                if (awaiter.State == AwaiterState.Started)
                    awaiter.Cancel();
            }
            _awaiters.Clear();
            while (_testAwaiters.TryDequeue(out IAwaiter awaiter))
            {
                if (awaiter.State == AwaiterState.Started)
                    awaiter.Cancel();
            }
        }

        private void Update()
        {
            while (_testAwaiters.TryDequeue(out IAwaiter awaiter))
            {
                _awaiters.Add(awaiter);
            }

            _awaiters.RemoveAll(awaiter => awaiter.State != AwaiterState.Started);
            foreach (IAwaiter awaiter in _awaiters)
            {
                if (awaiter.State != AwaiterState.Started)
                    continue;

                try
                {
                    awaiter.MoveNext();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Count = _awaiters.Count;
        }
    }
}
