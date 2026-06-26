using System;
using UnityEngine;

namespace Shears
{
    public delegate void RefChangeEvent<T>(in RefChangeData<T> data);

    public readonly struct RefChangeData<T>
    {
        public readonly T oldValue;
        public readonly T newValue;

        public RefChangeData(T oldValue, T newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }

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

                Changed?.Invoke(new(oldValue, value));
                ChangedRaw?.Invoke(value);
            }
        }

        public event RefChangeEvent<T> Changed;
        public event Action<T> ChangedRaw;

        public void Bind(RefChangeEvent<T> action)
        {
            Changed += action;
            action(new(value, value));
        }

        public void BindRaw(Action<T> action)
        {
            ChangedRaw += action;
            action(value);
        }

        public void Unbind(RefChangeEvent<T> action)
        {
            Changed -= action;
        }

        void IRef.Unbind(object changeEvent)
        {
            if (changeEvent is not RefChangeEvent<T> typedEvent)
                return;

            Changed -= typedEvent;
        }

        public void UnbindRaw(object changeEvent)
        {
            if (changeEvent is not Action<T> typedEvent)
                return;

            ChangedRaw -= typedEvent;
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

        public static bool operator ==(Ref<T> a, T b) =>
            a.value == null ? b == null : a.value.Equals(b);

        public static bool operator !=(Ref<T> a, T b) => !(a == b);

        public static implicit operator T(Ref<T> reference) => reference.value;
        #endregion
    }

    public interface IReadOnlyRef<T> : IRef
    {
        public T Value { get; }

        public event RefChangeEvent<T> Changed;
        public event Action<T> ChangedRaw;

        public void Bind(RefChangeEvent<T> action);
        public void BindRaw(Action<T> action);
    }

    public interface IRef
    {
        public void Unbind(object changeEvent);
        public void UnbindRaw(object changeEvent);
    }
}
