using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public sealed class Ref<T> : IReadOnlyRef<T>
    {
        public static readonly bool SerializeByReference =
            typeof(T).IsInterface || typeof(T).IsAbstract;

        [SerializeField]
        private T value;

        [SerializeField]
        private T refValue;

        private T SerializedValue
        {
            get => SerializeByReference ? refValue : value;
            set
            {
                if (SerializeByReference)
                    refValue = value;
                else
                    this.value = value;
            }
        }

        public T Value
        {
            get => SerializedValue;
            set
            {
                if (EqualityComparer<T>.Default.Equals(SerializedValue, value))
                    return;

                SerializedValue = value;
                Changed?.Invoke(value);
            }
        }
        public bool HasValue => Value != null;

        public event Action<T> Changed;

        public Ref()
        {
            SerializedValue = default;
        }

        public Ref(T value)
        {
            SerializedValue = value;
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
