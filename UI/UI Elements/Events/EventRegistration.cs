using System;
using System.Collections.Generic;

namespace Shears.UI
{
    public interface IEventRegistration
    {
        public void Register();

        public void Deregister();
    }

    public readonly struct EventRegistration<E> : IEventRegistration where E : IUIEvent
    {
        private readonly Action<E> callback;

        public readonly Action<E> Callback => callback;

        public EventRegistration(Action<E> callback)
        {
            this.callback = callback;
        }

        public void Register()
        {
            UIElementEventSystem.RegisterEvent(this);
        }

        public void Deregister()
        {
            UIElementEventSystem.DeregisterEvent(this);
        }

        public void Invoke(E evt) => callback(evt);

        #region Operator Overrides
        public override bool Equals(object obj)
        {
            return obj is EventRegistration<E> registration &&
                   EqualityComparer<Action<E>>.Default.Equals(callback, registration.callback);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(callback);
        }

        public static bool operator==(EventRegistration<E> e1, EventRegistration<E> e2)
            => e1.callback == e2.callback;

        public static bool operator!=(EventRegistration<E> e1, EventRegistration<E> e2)
            => !(e1 == e2);
        #endregion
    }
}
