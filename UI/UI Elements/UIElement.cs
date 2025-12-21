using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        private readonly List<IEventRegistration> registrations = new();

        private bool isEnabled = false;

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
    
        private void SetLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }
}
