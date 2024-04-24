// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ybwork.Async.Awaiters
{
    public abstract class AwaiterBase : INotifyCompletion
    {
        protected Action Continuation;
        public bool IsCompleted { get; protected set; } = false;

        protected AwaiterBase()
        {
            TaskManager.Instance.AddTaskAwaiter(this);
        }

        public virtual bool MoveNext()
        {
            if (IsCompleted)
            {
                Continuation?.Invoke();
                Continuation = null;
                return false;
            }
            return true;
        }

        public void GetResult() { }

        public virtual void SetValue(object result)
        {
            Complete();
        }

        protected void Complete()
        {
            IsCompleted = true;
            MoveNext();
        }

        public void SetException()
        {
            IsCompleted = true;
        }

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation.Invoke();
            }
            else
            {
                Continuation += continuation;
            }
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

        public override bool MoveNext()
        {
            if (IsCompleted)
                return false;

            if (!_predicate.Invoke())
            {
                return true;
            }
            else
            {
                IsCompleted = true;
                Continuation?.Invoke();
                Continuation = null;
                return false;
            }
        }
    }

    internal class DeleyAwaiter : AwaiterBase
    {
        private readonly float endtime;

        internal DeleyAwaiter(float duration) : base()
        {
            endtime = Time.time + duration;
        }

        public override bool MoveNext()
        {
            IsCompleted = endtime <= Time.time;
            return base.MoveNext();
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

        public override bool MoveNext()
        {
            IsCompleted = _currentFrame >= _frameCount;
            _currentFrame++;
            return base.MoveNext();
        }
    }

    internal class YieldAwaiter : AwaiterBase
    {
        private bool _isDone;
        internal YieldAwaiter() : base() { }

        public override bool MoveNext()
        {
            IsCompleted = _isDone;
            _isDone = true;
            return base.MoveNext();
        }
    }

    public class Awaiter : AwaiterBase
    {
        public object Result { get; private set; }

        public new object GetResult()
        {
            return Result;
        }

        public override void SetValue(object result)
        {
            Result = result;
            Complete();
        }
    }

    public class Awaiter<T> : AwaiterBase
    {
        public T Result { get; private set; }

        public new T GetResult()
        {
            return Result;
        }

        public void SetValue(T result)
        {
            Result = result;
            Complete();
        }

        public override void SetValue(object result)
        {
            SetValue((T)result);
        }
    }
}
