using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        private readonly Dictionary<Type, object> registrations = new();
        private bool isEnabled = false;
        private float dragBeginTime = 0.1f;

        public bool IsEnabled => isEnabled;
        public float DragBeginTime { get => dragBeginTime; set => dragBeginTime = value; }

        protected virtual void Awake()
        {
            Enable();

            RegisterEvents();
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            gameObject.SetActive(true);

            isEnabled = true;
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            gameObject.SetActive(false);

            isEnabled = false;
        }

        private void OnValidate()
        {
            Invoke(nameof(SetLayer), 0f);
        }

        public void RegisterEvent<EventType>(Action<EventType> callback)
            where EventType : struct, IUIEvent
        {
            var eventType = typeof(EventType);

            if (!registrations.TryGetValue(eventType, out var list))
            {
                list = new List<IEventRegistration<EventType>>();
                registrations[eventType] = list;
            }

            ((List<IEventRegistration<EventType>>)list).Add(new EventRegistration<EventType>(callback));
        }

        public void DeregisterEvent<EventType>(Action<EventType> callback)
            where EventType: struct, IUIEvent
        {
            var eventType = typeof(EventType);

            if (!registrations.TryGetValue(eventType, out var list))
                return;

            ((List<IEventRegistration<EventType>>)list).Remove(new EventRegistration<EventType>(callback));
        }

        internal void InvokeEvent<EventType>(EventType evt)
            where EventType : struct, IUIEvent
        {
            if (!registrations.TryGetValue(typeof(EventType), out var list))
                return;

            foreach (var registration in (List<IEventRegistration<EventType>>)list)
                registration.Invoke(evt);
        }

        protected virtual void RegisterEvents() { }
    
        private void SetLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }
}
