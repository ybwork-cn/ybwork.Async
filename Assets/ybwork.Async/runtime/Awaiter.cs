// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ybwork.Async.Awaiters
{
    internal abstract class AwaiterBase : IAwaiter
    {
        internal Action _continuation;
        public bool IsCompleted => State == AwaiterState.Completed;
        private AwaiterState _state;
        public AwaiterState State
        {
            get => _state;
            private protected set
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
                        AwaiterState.Aborted => AwaiterState.Aborted,
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
            YueTaskManager.AddTaskAwaiter(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAwaiter.MoveNext()
        {
            OnMoveNext();
            if (State == AwaiterState.Completed)
                Complete();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Complete()
        {
            _continuation?.Invoke();
            _continuation = null;
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnMoveNext()
        {
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAwaiter.SetException() => State = AwaiterState.Error;

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation)
        {
            switch (_state)
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

    internal class Awaiter : AwaiterBase, IAwaiterVoid
    {
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IAwaiterVoid.SetValue()
        {
            State = AwaiterState.Completed;
            Complete();
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
    }

    internal class CompletedAwaiter : Awaiter
    {
        internal CompletedAwaiter() : base()
        {
            State = AwaiterState.Completed;
        }
    }

    internal class WaitUntilAwater : Awaiter
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

    internal class DeleyAwaiter : Awaiter
    {
        private readonly float _duration;
        private float _elapsedTime = 0;

        internal DeleyAwaiter(float duration) : base()
        {
            _duration = duration;
        }

        protected override void OnMoveNext()
        {
            _elapsedTime += Time.deltaTime;
            if (_duration <= _elapsedTime)
                State = AwaiterState.Completed;
        }
    }

    internal class DeleyFramesAwaiter : Awaiter
    {
        private readonly float _frameCount;
        private int _currentFrame = 0;

        internal DeleyFramesAwaiter(int frameCount) : base()
        {
            _frameCount = frameCount;
        }

        protected override void OnMoveNext()
        {
            _currentFrame++;
            if (_currentFrame >= _frameCount)
                State = AwaiterState.Completed;
        }
    }

    internal class YieldAwaiter : Awaiter
    {
        internal YieldAwaiter() : base() { }

        protected override void OnMoveNext()
        {
            State = AwaiterState.Completed;
        }
    }

    internal class MutiAwaiter : Awaiter
    {
        public enum WaiteType
        {
            WaitAll,
            WaitAny,
        }

        private int _restCount;

        public MutiAwaiter(IReadOnlyCollection<YueTask> tasks, WaiteType type) : base()
        {
            _restCount = type switch
            {
                WaiteType.WaitAll => tasks.Count,
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

    internal class MutiAwaiter<T> : Awaiter<T[]>
    {
        private int _restCount;
        private readonly IReadOnlyCollection<YueTask<T>> _tasks;
        public MutiAwaiter(IReadOnlyCollection<YueTask<T>> tasks, MutiAwaiter.WaiteType type) : base()
        {
            this._tasks = tasks;
            _restCount = type switch
            {
                MutiAwaiter.WaiteType.WaitAll => tasks.Count,
                MutiAwaiter.WaiteType.WaitAny => 1,
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
            {
                var result = new T[_tasks.Count];
                int i = 0;
                foreach (YueTask<T> item in _tasks)
                {
                    result[i] = item.GetAwaiter().GetResult();
                    i++;
                }
                SetValue(result);
            }
        }
    }

    internal class Awaiter<T> : AwaiterBase, IAwaiter<T>
    {
        private T _result;

        public T GetResult()
        {
            return State switch
            {
                AwaiterState.Started => throw new InvalidOperationException("YueTask未完成"),
                AwaiterState.Completed => _result,
                AwaiterState.Aborted => throw new InvalidOperationException("YueTask已取消"),
                AwaiterState.Error => throw new InvalidOperationException("YueTask已发生错误"),
                _ => throw new NotImplementedException(),
            };
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T result)
        {
            _result = result;
            State = AwaiterState.Completed;
            Complete();
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnCompleted(Action<T> continuation)
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
