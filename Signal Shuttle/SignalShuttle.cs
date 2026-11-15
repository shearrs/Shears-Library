using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Signals
{
    public static class SignalShuttle
    {
        private static readonly Dictionary<Type, ISignalBindings> signals = new();

        private interface ISignalBindings { }

        private sealed class SignalBindings<T> : ISignalBindings
            where T : struct, ISignal
        {
            private readonly List<SignalListener<T>> listeners = new();

            public void AddListener(SignalListener<T> listener)
            {
                listeners.Add(listener);
            }

            public void RemoveListener(SignalListener<T> listener)
            {
                listeners.Remove(listener);
            }

            public void Invoke(in T signal)
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

        public static void Register<T>(SignalListener<T> listener)
            where T : struct, ISignal
        {
            var type = typeof(T);

            if (!signals.TryGetValue(type, out var bindings))
            {
                bindings = new SignalBindings<T>();
                signals[type] = bindings;
            }

            ((SignalBindings<T>)bindings).AddListener(listener);
        }

        public static void Unregister<T>(SignalListener<T> listener)
            where T : struct, ISignal
        {
            if (signals.TryGetValue(typeof(T), out var bindings))
            {
                ((SignalBindings<T>)bindings).RemoveListener(listener);
            }
        }

        public static void Emit<T>(in T signal)
            where T : struct, ISignal
        {
            if (signals.TryGetValue(typeof(T), out var bindings))
                ((SignalBindings<T>)bindings).Invoke(signal);
        }
    }
}
