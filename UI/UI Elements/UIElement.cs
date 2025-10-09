using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : MonoBehaviour
    {
        private readonly List<IEventRegistration> registrations = new();

        private void Awake()
        {
            AddEvents();
        }

        private void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            DeregisterEvents();
        }

        public void AddEvent<EventType>(Action<EventType> callback) where EventType : IUIEvent
        {
            var registration = new EventRegistration<EventType>(callback);
            registrations.Add(registration);

            if (enabled)
                registration.Register();
        }

        protected virtual void AddEvents() { }
    
        private void RegisterEvents()
        {
            foreach (var registration in registrations)
                registration.Register();
        }

        private void DeregisterEvents()
        {
            foreach (var registration in registrations)
                registration.Deregister();
        }
    }
}
