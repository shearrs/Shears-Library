using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    public class UIElementEventSystem : ProtectedSingleton<UIElementEventSystem>
    {
        private readonly Dictionary<Type, List<IEventRegistration>> registrations = new();

        public static void RegisterEvent<EventType>(EventRegistration<EventType> registration) where EventType : IUIEvent
            => Instance.InstRegisterEvent(registration);
        private void InstRegisterEvent<EventType>(EventRegistration<EventType> registration) where EventType : IUIEvent
        {
            var type = typeof(EventType);

            if (registrations.TryGetValue(type, out var list))
                list.Add(registration);
            else
                registrations.Add(type, new() { registration });
        }

        public static void DeregisterEvent<EventType>(EventRegistration<EventType> registration) where EventType : IUIEvent
            => Instance.InstDeregisterEvent(registration);
        private void InstDeregisterEvent<EventType>(EventRegistration<EventType> registration) where EventType : IUIEvent
        {
            var type = typeof(EventType);

            if (registrations.TryGetValue(type, out var list))
                list.Remove(registration);
        }
    }
}
