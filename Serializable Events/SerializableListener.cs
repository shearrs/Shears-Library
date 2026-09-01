using UnityEngine;

namespace Shears.Events
{
    /// <summary>
    /// A listener for a <see cref="SerializableEvent"/>.
    /// </summary>
    public abstract class SerializableListener : MonoBehaviour
    {
        /// <summary>
        /// Called by <see cref="SerializableEvent"/> when the event is invoked.
        /// </summary>
        internal void InvokeEvent()
        {
            OnInvoke();
        }

        /// <summary>
        /// Concrete implementation of listener.
        /// </summary>
        protected abstract void OnInvoke();
    }
}
