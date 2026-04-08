using System;
using UnityEngine;

namespace Shears
{
    public readonly struct RefChangeEvent<T>
    {
        public readonly T oldValue;
        public readonly T newValue;

        public RefChangeEvent(T oldValue, T newValue)
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

        public event Action<RefChangeEvent<T>> Changed;
        public event Action<T> ChangedRaw;

        public void Bind(Action<RefChangeEvent<T>> action)
        {
            Changed += action;
            action(new(value, value));
        }

        public void Unbind(Action<RefChangeEvent<T>> action)
        {
            Changed -= action;
        }

        void IRef.Unbind(object changeEvent)
        {
            if (changeEvent is not Action<RefChangeEvent<T>> typedEvent)
                return;

            Changed -= typedEvent;
        }
    }

    public interface IReadOnlyRef<T> : IRef
    {
        public T Value { get; }

        public event Action<RefChangeEvent<T>> Changed;
        public event Action<T> ChangedRaw;

        public void Bind(Action<RefChangeEvent<T>> action);
    }

    public interface IRef
    {
        public void Unbind(object changeEvent);
    }
}
