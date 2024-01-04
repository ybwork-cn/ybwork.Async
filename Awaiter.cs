// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using System.Collections;
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

        public abstract bool MoveNext();
        public abstract void GetResult();
        public abstract void SetValue(object result);
        public abstract void SetException(Exception ex);

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

    public class Awaiter : AwaiterBase
    {
        protected readonly IEnumerator Action;
        protected object Result { get; set; } = default;
        protected Exception Exception { get; set; }
        public override void GetResult()
        {
            if (IsCompleted)
                return;
            else
            {
                while (Action.MoveNext()) ;
                return;
            }
        }

        internal Awaiter() : base()
        {
            Action = LoopAciton();
        }

        internal Awaiter(IEnumerator enumerator) : base()
        {
            Action = enumerator;
        }

        public override bool MoveNext()
        {
            if (Exception != null)
                throw Exception;
            if (IsCompleted)
            {
                IsCompleted = true;
                Continuation?.Invoke();
                Continuation = null;
                return false;
            }
            if (Action.MoveNext())
            {
                return true;
            }
            else
            {
                IsCompleted = true;
                Result = Action.Current;
                Continuation?.Invoke();
                Continuation = null;
                return false;
            }
        }

        private IEnumerator LoopAciton()
        {
            while (!IsCompleted)
                yield return null;
        }

        public override void SetValue(object result)
        {
            Result = result;
            IsCompleted = true;
            MoveNext();
        }

        public override void SetException(Exception ex)
        {
            IsCompleted = true;
            Exception = ex;
            MoveNext();
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
            IsCompleted = endtime < Time.time;
            if (IsCompleted)
            {
                Continuation?.Invoke();
                Continuation = null;
                return false;
            }
            return true;
        }

        public override void GetResult()
        {
            if (IsCompleted)
                return;
            else
            {
                while (MoveNext()) ;
                return;
            }
        }

        public override void SetException(Exception ex)
        {
        }

        public override void SetValue(object result)
        {
        }
    }

    public class Awaiter<T> : Awaiter
    {
        public Awaiter() : base() { }

        public new T GetResult()
        {
            object result;
            if (IsCompleted)
                result = Result;
            else
            {
                while (Action.MoveNext()) ;
                result = Action.Current;
            }

            if (result is T t)
                return t;
            else return default;
        }

        public void SetValue(T result)
        {
            Result = result;
            IsCompleted = true;
            MoveNext();
        }
    }
}
