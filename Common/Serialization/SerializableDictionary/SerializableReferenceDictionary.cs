using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
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

                Add(entry.Key, entry.Value);
            }
        }
    }

    [System.Serializable]
    public class SerializableReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeReference] private List<TKey> keys = new();
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
