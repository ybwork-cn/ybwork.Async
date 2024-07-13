// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ybwork.Async.Awaiters
{
    public class AwaiterBase : INotifyCompletion
    {
        public enum AwaiterState
        {
            Started,
            Completed,
            Aborted,
            Error,
        }

        internal Action _continuation;
        public bool IsCompleted => State != AwaiterState.Started;
        private AwaiterState _state;
        public AwaiterState State
        {
            get => _state;
            protected set
            {
                if (value == AwaiterState.Started)
                {
                    throw new MulticastNotSupportedException("不支持多次尝试开始一个YueTask");
                }
                else if (value == AwaiterState.Completed)
                {
                    _state = _state switch
                    {
                        AwaiterState.Started => AwaiterState.Completed,
                        AwaiterState.Completed => throw new MulticastNotSupportedException("不支持多次尝试完成一个YueTask"),
                        AwaiterState.Aborted => throw new InvalidOperationException("不支持尝试完成一个已取消的YueTask"),
                        AwaiterState.Error => throw new InvalidOperationException("不支持尝试完成一个已抛出错误的YueTask"),
                        _ => throw new NotImplementedException(),
                    };
                }
                else if (value == AwaiterState.Aborted)
                {
                    _state = _state switch
                    {
                        AwaiterState.Started => AwaiterState.Aborted,
                        AwaiterState.Completed => throw new InvalidOperationException("不支持尝试取消一个已完成的YueTask"),
                        AwaiterState.Aborted => throw new MulticastNotSupportedException("不支持多次尝试取消一个YueTask"),
                        AwaiterState.Error => throw new InvalidOperationException("不支持尝试取消一个已抛出错误的YueTask"),
                        _ => throw new NotImplementedException(),
                    };
                }
                else if (value == AwaiterState.Error)
                {
                    _state = AwaiterState.Error;
                }
            }
        }

        internal AwaiterBase()
        {
            _state = AwaiterState.Started;
            YueTaskManager.Instance.AddTaskAwaiter(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool MoveNext()
        {
            OnMoveNext();
            if (State == AwaiterState.Completed)
                Complete();
            return !IsCompleted;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnMoveNext()
        {
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult()
        {
            switch (State)
            {
                case AwaiterState.Started:
                    throw new InvalidOperationException("YueTask未完成");
                case AwaiterState.Completed:
                    break;
                case AwaiterState.Aborted:
                    throw new InvalidOperationException("YueTask已取消");
                case AwaiterState.Error:
                    throw new InvalidOperationException("YueTask已发生错误");
                default:
                    throw new NotImplementedException();
            }
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetValue()
        {
            State = AwaiterState.Completed;
            Complete();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Complete()
        {
            _continuation?.Invoke();
            _continuation = null;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetException() => State = AwaiterState.Error;

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            switch (State)
            {
                case AwaiterState.Started:
                    _continuation += continuation;
                    break;
                case AwaiterState.Completed:
                    continuation?.Invoke();
                    break;
                case AwaiterState.Aborted:
                    throw new InvalidOperationException("YueTask已取消");
                case AwaiterState.Error:
                    throw new InvalidOperationException("YueTask已发生错误");
                default:
                    throw new NotImplementedException();
            }
        }

        public void Cancel()
        {
            State = AwaiterState.Aborted;
        }
    }

    internal class CompletedAwaiter : AwaiterBase
    {
        internal CompletedAwaiter() : base()
        {
            State = AwaiterState.Completed;
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
        protected override void OnMoveNext()
        {
            if (_predicate.Invoke())
                State = AwaiterState.Completed;
        }
    }

    internal class DeleyAwaiter : AwaiterBase
    {
        private readonly float _endtime;

        internal DeleyAwaiter(float duration) : base()
        {
            _endtime = Time.time + duration;
        }

        protected override void OnMoveNext()
        {
            if (_endtime <= Time.time)
                State = AwaiterState.Completed;
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

        protected override void OnMoveNext()
        {
            if (_currentFrame >= _frameCount)
                State = AwaiterState.Completed;

            _currentFrame++;
        }
    }

    internal class YieldAwaiter : AwaiterBase
    {
        private bool _isDone = false;
        internal YieldAwaiter() : base() { }

        protected override void OnMoveNext()
        {
            if (_isDone)
                State = AwaiterState.Completed;

            _isDone = true;
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

        protected override void OnMoveNext()
        {
            if (_restCount <= 0)
                State = AwaiterState.Completed;
        }
    }

    public class Awaiter<T> : AwaiterBase
    {
        private T _result;

        public new T GetResult()
        {
            base.GetResult();
            return _result;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            _result = result;
            SetValue();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action<T> continuation)
        {
            switch (State)
            {
                case AwaiterState.Started:
                    _continuation += () => continuation.Invoke(_result);
                    break;
                case AwaiterState.Completed:
                    continuation?.Invoke(_result);
                    break;
                case AwaiterState.Aborted:
                    throw new InvalidOperationException("YueTask已取消");
                case AwaiterState.Error:
                    throw new InvalidOperationException("YueTask已发生错误");
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
