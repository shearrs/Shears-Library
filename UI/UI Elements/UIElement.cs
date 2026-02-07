using Shears.Logging;
using Shears.Tweens;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElement : SHMonoBehaviourLogger
    {
        private readonly Dictionary<Type, object> registrations = new();
        private readonly List<UIElement> childElements = new();
        private readonly List<Tween> tweens = new();
        private bool isEnabled = false;
        private float dragBeginTime = 0.1f;

        protected IReadOnlyList<Tween> Tweens => tweens;
        public bool IsEnabled => isEnabled;
        public float DragBeginTime { get => dragBeginTime; set => dragBeginTime = value; }

        public event Action Disabled;

        protected virtual void Awake()
        {
            Enable();

            RegisterEvents();
        }

        private void OnDisable()
        {
            Disabled?.Invoke();
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
            where EventType : UIEvent
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
            where EventType: UIEvent
        {
            var eventType = typeof(EventType);

            if (!registrations.TryGetValue(eventType, out var list))
                return;

            ((List<IEventRegistration<EventType>>)list).Remove(new EventRegistration<EventType>(callback));
        }

        internal void InvokeEvent<EventType>(EventType evt)
            where EventType : UIEvent
        {
            if (registrations.TryGetValue(typeof(EventType), out var list))
            {
                foreach (var registration in (List<IEventRegistration<EventType>>)list)
                    registration.Invoke(evt);
            }

            if (!evt.WillTrickleDown)
                return;

            GetComponentsInChildren(childElements);

            foreach (var child in childElements)
            {
                if (child == this)
                    continue;

                child.InvokeEvent(evt);
            }
        }

        public void Focus() => UIElementEventSystem.Focus(this);

        public void Blur() => UIElementEventSystem.Focus(null);

        protected void StoreTween(Tween tween) => tweens.Add(tween);

        protected void DisposeTweens()
        {
            for (int i = 0; i < tweens.Count; i++)
                tweens[i].Dispose();

            tweens.Clear();
        }

        protected virtual void RegisterEvents() { }
    
        private void SetLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }
}
