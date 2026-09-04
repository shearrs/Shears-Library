using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Shears
{
    public interface IInterfaceSerializable : ISerializationCallbackReceiver
    {
        private static readonly Dictionary<Type, FieldInfo[]> interfaceFieldCache = new();

        protected InterfaceDictionary InterfaceEntries { get; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var fields = GetCachedInterfaceFields(GetType());

            InterfaceEntries.Clear();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(this);
                InterfaceEntry entry;

                if (value is Object unityObject)
                    entry = new(unityObject, null);
                else
                    entry = new(null, value);

#if UNITY_EDITOR
                entry.FieldName = field.Name;
                entry.FieldType = field.FieldType;
#endif

                InterfaceEntries[field.Name] = entry;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Profiler.BeginSample("Interface Deserialization");

            if (InterfaceEntries == null || InterfaceEntries.Count == 0)
                return;

            Profiler.BeginSample("Get Cached Interface Fields");
            var fields = GetCachedInterfaceFields(GetType());
            Profiler.EndSample();

            Profiler.BeginSample("Set Field Values");
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                if (!InterfaceEntries.TryGetValue(field.Name, out var entry))
                    continue;

                var interfaceType = field.FieldType;

                if (
                    !ReferenceEquals(entry.UnityObject, null)
                    && interfaceType.IsAssignableFrom(entry.UnityObject.GetType())
                )
                    field.SetValue(this, entry.UnityObject);
                else if (entry.RawObject != null)
                    field.SetValue(this, entry.RawObject);
                else
                    field.SetValue(this, null);
            }
            Profiler.EndSample();

            Profiler.EndSample();
        }

        private static FieldInfo[] GetCachedInterfaceFields(Type type)
        {
            if (!interfaceFieldCache.TryGetValue(type, out var fields))
            {
                var allFields = type.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                var filteredList = new List<FieldInfo>();

                foreach (var field in allFields)
                {
                    if (
                        field.FieldType.IsInterface && field.IsDefined(typeof(SerializeField), true)
                    )
                        filteredList.Add(field);
                }

                fields = filteredList.ToArray();
                interfaceFieldCache[type] = fields;
            }

            return fields;
        }
    }

    [Serializable]
    public class InterfaceDictionary : SerializableDictionary<string, InterfaceEntry> { }
}
