// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ybwork.Async.Awaiters;

namespace ybwork.Async
{
    public sealed class YueTaskManager : MonoBehaviour
    {
        private static int? _mainThradId = null;
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
            if (!Application.isPlaying)
                throw new InvalidOperationException($"禁止在编辑器退出后调用{nameof(YueTaskManager)}");
            if (_instance == null)
                _instance = FindObjectOfType<YueTaskManager>();
            if (_instance == null)
            {
                _instance = new GameObject(nameof(YueTaskManager)).AddComponent<YueTaskManager>();
                DontDestroyOnLoad(_instance);
            }
            _mainThradId ??= Thread.CurrentThread.ManagedThreadId;
        }

        internal static void AddTaskAwaiter(IAwaiter taskAwaiter)
        {
            if (Instance != null)
                Instance._testAwaiters.Enqueue(taskAwaiter);
        }

        public int Count;
        private readonly List<IAwaiter> _awaiters = new();
        private readonly ConcurrentQueue<IAwaiter> _testAwaiters = new();
        private bool _isClearing = false;

        /// <summary>
        /// 停止所有正在运行的YueTask
        /// </summary>
        public void CancelAllTask()
        {
            if (!IsMainThread)
            {
                _isClearing = true;
                return;
            }
            foreach (IAwaiter awaiter in _awaiters)
            {
                if (awaiter.State == AwaiterState.Started)
                    awaiter.Cancel();
            }
            _testAwaiters.Clear();
        }

        internal static bool IsMainThread
        {
            get
            {
                if (_mainThradId == null)
                    throw new InvalidOperationException("请先在Unity主线程访问一次YueTaskManager.Instance或者创建一次YueTask");
                return _mainThradId == Thread.CurrentThread.ManagedThreadId;
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
                if (_isClearing)
                    break;

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

            if (_isClearing)
            {
                _awaiters.Clear();
                _testAwaiters.Clear();
                _isClearing = false;
            }

            Count = _awaiters.Count;
        }
    }
}
