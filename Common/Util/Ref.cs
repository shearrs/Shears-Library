using System;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public sealed class Ref<T> : IReadOnlyRef<T>
    {
        [SerializeField]
        private T value;

        public T Value
        {
            get => value;
            set
            {
                var oldValue = this.value;
                this.value = value;

                Changed?.Invoke(value);
            }
        }
        public bool HasValue => Value != null;

        public event Action<T> Changed;

        public Ref()
        {
            value = default;
        }

        public Ref(T value)
        {
            this.value = value;
        }

        public void Bind(Action<T> action)
        {
            Changed += action;
            action(value);
        }

        public void Unbind(Action<T> action)
        {
            Changed -= action;
        }

        void IRef.Unbind(object changeEvent)
        {
            if (changeEvent is not Action<T> typedEvent)
                return;

            Changed -= typedEvent;
        }

        #region Operators
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Ref<T> a, T b)
        {
            if (a == null || a.value == null)
                return b == null;
            else
                return a.value.Equals(b);
        }

        public static bool operator !=(Ref<T> a, T b) => !(a == b);
        #endregion
    }

    public interface IReadOnlyRef<T> : IRef
    {
        public T Value { get; }
        public bool HasValue => Value != null;

        public event Action<T> Changed;

        public void Bind(Action<T> action);

        public void Unbind(Action<T> action);
    }

    public interface IRef
    {
        public void Unbind(object changeEvent);
    }
}
