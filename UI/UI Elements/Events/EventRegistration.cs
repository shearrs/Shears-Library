using System;
using System.Collections.Generic;

namespace Shears.UI
{
    public interface IEventRegistration<T> where T : UIEvent
    {
        public void Invoke(in T evt);
    }

    public readonly struct EventRegistration<T> : IEventRegistration<T> where T : UIEvent
    {
        private readonly Action<T> callback;

        public readonly Action<T> Callback => callback;

        public EventRegistration(Action<T> callback)
        {
            this.callback = callback;
        }

        void IEventRegistration<T>.Invoke(in T evt)
        {
            callback(evt);
        }

        #region Operator Overrides
        public override bool Equals(object obj)
        {
            return obj is EventRegistration<T> registration &&
                   EqualityComparer<Action<T>>.Default.Equals(callback, registration.callback);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(callback);
        }

        public static bool operator==(EventRegistration<T> e1, EventRegistration<T> e2)
            => e1.callback == e2.callback;

        public static bool operator!=(EventRegistration<T> e1, EventRegistration<T> e2)
            => !(e1 == e2);
        #endregion
    }
}
