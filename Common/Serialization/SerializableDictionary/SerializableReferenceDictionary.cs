using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A serializable entry in a <see cref="SerializableDictionaryEntry{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    [System.Serializable]
    internal struct SerializableDictionaryEntry<TKey, TValue>
    {
        [SerializeField] private TKey key;
        [SerializeField] private TValue value;

        public readonly TKey Key => key;
        public readonly TValue Value => value;

        public SerializableDictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    /// <summary>
    /// A serializable dictionary that can be used in the Unity Inspector.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            entries.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
                entries.Add(new(pair.Key, pair.Value));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            int entryCount = entries.Count;

            for (int i = 0; i < entryCount; i++)
            {
                var entry = entries[i];
                var key = entry.Key;

                if (ContainsKey(key))
                    key = default;

                Add(key, entry.Value);
            }
        }
    }

    /// <summary>
    /// A serializable dictionary that can be used in the Unity Inspector, with support Unity's <see cref="SerializeReference"/> attribute..
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    [System.Serializable]
    public class SerializableReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new();
        [SerializeReference] private List<TValue> values = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            int keyCount = keys.Count;
            int valueCount = values.Count;

            if (keyCount != valueCount)
                throw new System.Exception("Number of keys not equal to number of values! Make sure both types are serializable.");

            for (int i = 0; i < keyCount; i++)
                Add(keys[i], values[i]);
        }
    }
}
