// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ybwork.Async.Awaiters
{
    public class AwaiterBase : INotifyCompletion
    {
        internal Action _continuation;
        public bool IsCompleted { get; protected private set; } = false;

        internal AwaiterBase()
        {
            TaskManager.Instance.AddTaskAwaiter(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual bool MoveNext()
        {
            return !IsCompleted;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            if (!IsCompleted)
                throw new InvalidOperationException("YueTask未完成");
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetValue()
        {
            Complete();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Complete()
        {
            IsCompleted = true;
            _continuation?.Invoke();
            _continuation = null;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetException() => IsCompleted = true;

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation.Invoke();
            }
            else
            {
                _continuation += continuation;
            }
        }
    }

    internal class CompletedAwaiter : AwaiterBase
    {
        internal CompletedAwaiter() : base()
        {
            IsCompleted = true;
        }
    }

    internal class WaitUntilAwater : AwaiterBase
    {
        private readonly Func<bool> _predicate;

        internal WaitUntilAwater(Func<bool> predicate) : base()
        {
            _predicate = predicate;
            if (_predicate == null)
                throw new NullReferenceException("predicate参数未绑定任何方法");
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override bool MoveNext()
        {
            if (IsCompleted)
                return false;
            IsCompleted = _predicate.Invoke();
            return !IsCompleted;
        }
    }

    internal class DeleyAwaiter : AwaiterBase
    {
        private readonly float _endtime;

        internal DeleyAwaiter(float duration) : base()
        {
            _endtime = Time.time + duration;
        }

        internal override bool MoveNext()
        {
            IsCompleted = _endtime <= Time.time;
            return !IsCompleted;
        }
    }

    internal class DeleyFramesAwaiter : AwaiterBase
    {
        private readonly float _frameCount;
        private int _currentFrame = 0;

        internal DeleyFramesAwaiter(int frameCount) : base()
        {
            _frameCount = frameCount;
        }

        internal override bool MoveNext()
        {
            IsCompleted = _currentFrame >= _frameCount;
            _currentFrame++;
            return !IsCompleted;
        }
    }

    internal class YieldAwaiter : AwaiterBase
    {
        private bool _isDone;
        internal YieldAwaiter() : base() { }

        internal override bool MoveNext()
        {
            IsCompleted = _isDone;
            _isDone = true;
            return !IsCompleted;
        }
    }

    internal class MutiAwaiter : AwaiterBase
    {
        public enum WaiteType
        {
            WaitAll,
            WaitAny,
        }

        private int _restCount;

        public MutiAwaiter(YueTask[] tasks, WaiteType type) : base()
        {
            _restCount = type switch
            {
                WaiteType.WaitAll => tasks.Length,
                WaiteType.WaitAny => 1,
                _ => throw new NotImplementedException(),
            };

            foreach (YueTask task in tasks)
            {
                task.Then(OnItemTaskEnd);
            }
        }

        private void OnItemTaskEnd()
        {
            _restCount--;
        }

        internal override bool MoveNext()
        {
            IsCompleted = _restCount <= 0;
            return !IsCompleted;
        }
    }

    public class Awaiter<T> : AwaiterBase
    {
        private T _result;

        public new T GetResult()
        {
            if (!IsCompleted)
                throw new InvalidOperationException("YueTask未完成");
            return _result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            _result = result;
            Complete();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<T> continuation)
        {
            if (IsCompleted)
                continuation.Invoke(_result);
            else
                _continuation += () => continuation.Invoke(_result);
        }
    }
}
