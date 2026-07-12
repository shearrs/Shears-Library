using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// A serializable entry in a <see cref="SerializableDictionaryEntry{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    [Serializable]
    public struct SerializableDictionaryEntry<TKey, TValue>
    {
        [SerializeField]
        private TKey key;

        [SerializeField]
        private TValue value;

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
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>,
            ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (entry.Key != null && !ContainsKey(entry.Key))
                {
                    entries.Remove(entry);
                    i--;
                }
            }

            foreach (var pair in this)
            {
                bool hasEntry = false;

                foreach (var entryPair in entries)
                {
                    if (EqualityComparer<TKey>.Default.Equals(pair.Key, entryPair.Key))
                    {
                        hasEntry = true;
                        break;
                    }
                }

                if (hasEntry)
                    continue;

                var entry = new SerializableDictionaryEntry<TKey, TValue>(pair.Key, pair.Value);
                entries.Add(entry);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            int entryCount = entries.Count;

            for (int i = 0; i < entryCount; i++)
            {
                var entry = entries[i];
                var key = entry.Key;

                if (key == null)
                    continue;

                if (ContainsKey(key))
                {
                    if (typeof(TKey) == typeof(string))
                    {
                        key = (TKey)(Guid.NewGuid().ToString() as object);
                        entries[i] = new(key, entry.Value);
                    }
                    else
                    {
                        key = default;
                        entries[i] = new(key, entry.Value);
                    }
                }

                if (key == null)
                    continue;

                if (ContainsKey(key))
                {
                    string keyName = typeof(UnityEngine.Object).IsAssignableFrom(typeof(TKey))
                        ? "Unity Object"
                        : key.ToString();

                    Debug.LogWarning($"Dictionary already contains key: {keyName}");
                    continue;
                }

                Add(key, entry.Value);
            }
        }
    }

    /// <summary>
    /// A serializable dictionary that can be used in the Unity Inspector. Supports Unity's <see cref="SerializeReference"/> attribute.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    [System.Serializable]
    public class SerializableReferenceDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>,
            ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new();

        [SerializeReference]
        private List<TValue> values = new();
        private readonly List<KeyToRemove> keysToRemove = new();

        private readonly struct KeyToRemove
        {
            private readonly TKey key;
            private readonly TValue value;

            public readonly TKey Key => key;
            public readonly TValue Value => value;

            public KeyToRemove(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                if (keys.Contains(pair.Key))
                    continue;

                keys.Add(pair.Key);
                values.Add(pair.Value);
            }

            keysToRemove.Clear();

            for (int i = 0; i < keys.Count; i++)
            {
                if (!ContainsKey(keys[i]))
                    keysToRemove.Add(new(keys[i], values[i]));
            }

            foreach (var key in keysToRemove)
            {
                keys.Remove(key.Key);
                values.Remove(key.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            int keyCount = keys.Count;
            int valueCount = values.Count;

            if (keyCount != valueCount)
                throw new System.Exception(
                    "Number of keys not equal to number of values! Make sure both types are serializable."
                );

            for (int i = 0; i < keyCount; i++)
                Add(keys[i], values[i]);
        }
    }
}
