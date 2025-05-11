using UnityEngine;

namespace Shears.UI
{
    public abstract class ManagedWrapper : MonoBehaviour
    {
        public abstract Component WrappedValue { get; }

        protected virtual void OnDestroy()
        {
            if (WrappedValue == null || gameObject == null)
                return;

            Destroy(WrappedValue);
        }
    }

    public abstract class ManagedWrapper<T> : ManagedWrapper where T : Component
    {
        public override Component WrappedValue => TypedWrappedValue;

        public T TypedWrappedValue
        {
            get
            {
                return GetComponent<T>();
            }
        }
    }
}
