using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Signals
{
    public static class SignalShuttle
    {
        private static readonly Dictionary<Type, object> signals = new();

        private sealed class SignalBindings<TSignal> where TSignal : struct, ISignal
        {
            private readonly List<Action<TSignal>> listeners = new();

            public void AddListener(Action<TSignal> listener)
            {
                listeners.Add(listener);
            }

            public void RemoveListener(Action<TSignal> listener)
            {
                listeners.Remove(listener);
            }

            public void Invoke(in TSignal signal)
            {
                for (int i = 0; i < listeners.Count; i++)
                    listeners[i]?.Invoke(signal);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeSignals()
        {
            signals.Clear();
        }

        public static void Register<TSignal>(Action<TSignal> listener) where TSignal: struct, ISignal
        {
            var type = typeof(TSignal);

            if (!signals.TryGetValue(type, out var bindings))
            {
                bindings = new SignalBindings<TSignal>();
                signals[type] = bindings;
            }

            ((SignalBindings<TSignal>)bindings).AddListener(listener);
        }

        public static void Deregister<TSignal>(Action<TSignal> listener) where TSignal: struct, ISignal
        {
            if (signals.TryGetValue(typeof(TSignal), out var bindings))
            {
                ((SignalBindings<TSignal>)bindings).RemoveListener(listener);
            }
        }

        public static void Emit<TSignal>(TSignal signal) where TSignal : struct, ISignal
        {
            if (signals.TryGetValue(typeof(TSignal), out var bindings))
                ((SignalBindings<TSignal>)bindings).Invoke(signal);
        }
    }
}
