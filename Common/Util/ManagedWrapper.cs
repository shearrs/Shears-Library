using System;
using UnityEngine;

namespace Shears
{
    public abstract class ManagedWrapper : MonoBehaviour
    {
        public abstract Component WrappedValue { get; }

        public abstract Type GetWrappedType();

        protected virtual void OnDestroy()
        {
            if (WrappedValue == null || gameObject == null)
                return;

            Destroy(WrappedValue);
        }
    }

    public abstract class ManagedWrapper<T> : ManagedWrapper where T : Component
    {
        private T typedWrappedValue;

        public override Component WrappedValue => TypedWrappedValue;

        public T TypedWrappedValue
        {
            get
            {
                if (typedWrappedValue == null)
                    typedWrappedValue = GetComponent<T>();

                return typedWrappedValue;
            }
        }

        public override Type GetWrappedType() => typeof(T);
    }
}
