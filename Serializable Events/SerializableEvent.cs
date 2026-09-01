using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Events
{
    /// <summary>
    /// An event type that is serialized in the Unity inspector.<br/>
    /// Similar to a <see cref="UnityEngine.Events.UnityEvent"/>, but instead of reflection it uses concrete <see cref="SerializableListener"/> implementations.<br/><br/>
    /// Workflow:<br/>
    /// - Have a script with various serialized <see cref="SerializableEvent"/>s<br/>
    /// - Attach <see cref="SerializableListener"/> components<br/>
    /// - Add listeners through the inspector
    /// </summary>
    [Serializable]
    public class SerializableEvent
    {
        [SerializeField]
        [Tooltip("Listeners to notify when this event is invoked.")]
        private List<SerializableListener> listeners = new();

        /// <summary>
        /// Listeners to notify when this event is invoked.
        /// </summary>
        public IReadOnlyList<SerializableListener> Listeners => listeners;

        /// <summary>
        /// Add a listener to notify to this <see cref="SerializableEvent"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(SerializableListener listener)
        {
            listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener from this <see cref="SerializableEvent"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <returns>If the listener was listening to this event.</returns>
        public bool RemoveListener(SerializableListener listener)
        {
            return listeners.Remove(listener);
        }

        /// <summary>
        /// Invoke this event. Notifies all listeners.
        /// </summary>
        public void Invoke()
        {
            foreach (var listener in listeners)
                listener.InvokeEvent();
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEvent operator +(
            SerializableEvent evt,
            SerializableListener listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEvent operator -(
            SerializableEvent evt,
            SerializableListener listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }
    }
}
