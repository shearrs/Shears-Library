using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Events
{
    [Serializable]
    public abstract class SerializableEventHandle
    {
        [SerializeField]
        [Tooltip("Listeners to notify when this event is invoked.")]
        protected List<SerializableListener> listeners = new();

        protected readonly List<Action> rawListeners = new();

        /// <summary>
        /// Add a listener to notify to this <see cref="SerializableEventHandle"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(Action listener)
        {
            rawListeners.Add(listener);
        }

        /// <inheritdoc cref="AddListener(Action)"/>
        public void AddListener(SerializableListener listener)
        {
            listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener from this <see cref="SerializableEventHandle"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <returns>If the listener was listening to this event.</returns>
        public bool RemoveListener(Action listener)
        {
            return rawListeners.Remove(listener);
        }

        /// <inheritdoc cref="RemoveListener(Action)"/>
        public bool RemoveListener(SerializableListener listener)
        {
            return listeners.Remove(listener);
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEventHandle operator +(
            SerializableEventHandle evt,
            SerializableListener listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEventHandle operator -(
            SerializableEventHandle evt,
            SerializableListener listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEventHandle operator +(
            SerializableEventHandle evt,
            Action listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEventHandle operator -(
            SerializableEventHandle evt,
            Action listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }
    }

    [Serializable]
    public abstract class SerializableEventHandle<T>
    {
        [SerializeField]
        [Tooltip("Listeners to notify when this event is invoked.")]
        protected List<SerializableListener<T>> listeners = new();

        [SerializeField]
        [Tooltip(
            "Listeners to notify when this event is invoked. These listeners don't receive event data."
        )]
        protected List<SerializableListener> untypedListeners = new();

        protected readonly List<Action<T>> rawListeners = new();

        /// <summary>
        /// Add a listener to notify to this <see cref="SerializableEventHandle{T}"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(Action<T> listener)
        {
            rawListeners.Add(listener);
        }

        /// <inheritdoc cref="AddListener(Action{T})"/>
        public void AddListener(SerializableListener<T> listener)
        {
            listeners.Add(listener);
        }

        /// <inheritdoc cref="AddListener(Action{T})"/>
        public void AddListener(SerializableListener listener)
        {
            untypedListeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener from this <see cref="SerializableEventHandle{T}"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <returns>If the listener was listening to this event.</returns>
        public bool RemoveListener(Action<T> listener)
        {
            return rawListeners.Remove(listener);
        }

        /// <inheritdoc cref="RemoveListener(Action{T})"/>
        public bool RemoveListener(SerializableListener<T> listener)
        {
            return listeners.Remove(listener);
        }

        /// <inheritdoc cref="RemoveListener(Action{T})"/>
        public bool RemoveListener(SerializableListener listener)
        {
            return untypedListeners.Remove(listener);
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEventHandle<T> operator +(
            SerializableEventHandle<T> evt,
            SerializableListener<T> listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEventHandle<T> operator -(
            SerializableEventHandle<T> evt,
            SerializableListener<T> listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEventHandle<T> operator +(
            SerializableEventHandle<T> evt,
            Action<T> listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEventHandle<T> operator -(
            SerializableEventHandle<T> evt,
            Action<T> listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }

        /// <inheritdoc cref="AddListener"/>
        /// <param name="evt">The event to add to.</param>
        public static SerializableEventHandle<T> operator +(
            SerializableEventHandle<T> evt,
            SerializableListener listener
        )
        {
            evt.AddListener(listener);

            return evt;
        }

        /// <inheritdoc cref="RemoveListener"/>
        /// <param name="evt">The event to remove from.</param>
        public static SerializableEventHandle<T> operator -(
            SerializableEventHandle<T> evt,
            SerializableListener listener
        )
        {
            evt.RemoveListener(listener);

            return evt;
        }
    }
}
