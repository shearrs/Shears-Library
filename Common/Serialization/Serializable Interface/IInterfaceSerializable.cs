using System;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Shears
{
    /// <summary>
    /// Implement this to enable interface serialization.<br/>
    /// IMPORTANT: <see cref="Entries"/> needs to point to a serialized <see cref="InterfaceDictionary"/> field named "__interfaceEntries" (CASE SENSITIVE).<br/><br/>
    ///
    /// If the target object has its own custom editor, then that editor will need to call InterfaceSerializer.SerializeFields.
    /// </summary>
    public interface IInterfaceSerializable : ISerializationCallbackReceiver
    {
        /// <summary>
        /// A serialized mapping of interface values.
        /// </summary>
        protected InterfaceDictionary InterfaceEntries { get; }

        /// <summary>
        /// Serialize interface field values into <see cref="InterfaceEntry"/>.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var fields = InterfaceFieldCache.GetCachedInterfaceFields(GetType());

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

        /// <summary>
        /// Deserialize <see cref="InterfaceEntry"/>s into C# interface values.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Profiler.BeginSample("Interface Deserialization");

            if (InterfaceEntries == null || InterfaceEntries.Count == 0)
                return;

            Profiler.BeginSample("Get Cached Interface Fields");
            var fields = InterfaceFieldCache.GetCachedInterfaceFields(GetType());
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
    }

    /// <summary>
    /// Wrapper type for a string -> <see cref="InterfaceEntry"/> dictionary.
    /// </summary>
    [Serializable]
    public class InterfaceDictionary : SerializableDictionary<string, InterfaceEntry> { }
}
