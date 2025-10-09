using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        private readonly List<IEventRegistration> registrations = new();

        protected virtual void Awake()
        {
            RegisterEvents();
        }

        private void OnValidate()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }

        public void RegisterEvent<EventType>(Action<EventType> callback) where EventType : IUIEvent
        {
            registrations.Add(new EventRegistration<EventType>(callback));
        }

        public void DeregisterEvent<EventType>(Action<EventType> callback) where EventType: IUIEvent
        {
            registrations.Remove(new EventRegistration<EventType>(callback));
        }

        internal void InvokeEvent<EventType>(EventType evt) where EventType : IUIEvent
        {
            foreach (var registration in registrations)
                registration.TryInvoke(evt);
        }

        protected virtual void RegisterEvents() { }
    }
}
