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
    public class SerializableEvent : SerializableEventHandle
    {
        /// <summary>
        /// Serialized listeners to notify when this event is invoked.
        /// </summary>
        public IReadOnlyList<SerializableListener> Listeners => listeners;

        /// <summary>
        /// Normal C# listeners to notify when this event is invoked.
        /// </summary>
        public IReadOnlyList<Action> RawListeners => rawListeners;

        /// <summary>
        /// Invoke this event. Notifies all listeners.
        /// </summary>
        public void Invoke()
        {
            foreach (var listener in rawListeners)
                listener?.Invoke();

            foreach (var listener in listeners)
                listener.InvokeEvent();
        }
    }

    /// <summary>
    /// An event type that is serialized in the Unity inspector.<br/>
    /// Similar to a <see cref="UnityEngine.Events.UnityEvent"/>, but instead of reflection it uses concrete <see cref="SerializableListener"/> implementations.<br/><br/>
    /// Workflow:<br/>
    /// - Have a script with various serialized <see cref="SerializableEvent"/>s<br/>
    /// - Attach <see cref="SerializableListener"/> components<br/>
    /// - Add listeners through the inspector
    /// </summary>
    [Serializable]
    public class SerializableEvent<T> : SerializableEventHandle<T>
    {
        /// <summary>
        /// Serialized listeners to notify when this event is invoked.
        /// </summary>
        public IReadOnlyList<SerializableListener<T>> Listeners => listeners;

        /// <summary>
        /// Serialized listeners to notify when this event is invoked. These listeners don't receive event data.
        /// </summary>
        public IReadOnlyList<SerializableListener> UntypedListeners => untypedListeners;

        /// <summary>
        /// Normal C# listeners to notify when this event is invoked.
        /// </summary>
        public IReadOnlyList<Action<T>> RawListeners => rawListeners;

        /// <summary>
        /// Invoke this event. Notifies all listeners.
        /// </summary>
        public void Invoke(in T data)
        {
            foreach (var listener in rawListeners)
                listener?.Invoke(data);

            foreach (var listener in listeners)
                listener.InvokeEvent(data);

            foreach (var listener in untypedListeners)
                listener.InvokeEvent();
        }
    }
}
