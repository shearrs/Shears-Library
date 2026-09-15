using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// An entry in a <see cref="SerializableDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public struct SerializableDictionaryEntry<TKey, TValue>
    {
        [SerializeField]
        [Tooltip("The key value.")]
        private TKey key;

        [SerializeField]
        [Tooltip("The value.")]
        private TValue value;

        /// <summary>
        /// The key value.
        /// </summary>
        public readonly TKey Key => key;

        /// <summary>
        /// The value.
        /// </summary>
        public readonly TValue Value => value;

        public SerializableDictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    /// <summary>
    /// A serializable version of a standard C# <see cref="Dictionary{TKey, TValue}"/>. Can be used just like a normal dictionary, but compatible with serialization.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>,
            ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The entries of this dictionary.")]
        private List<SerializableDictionaryEntry<TKey, TValue>> entries = new();

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("Invalid values in this dictionary.")]
        private List<SerializableDictionaryEntry<TKey, TValue>> invalidEntries = new();
#endif

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            entries.Clear();

            foreach (var pair in this)
            {
                var entry = new SerializableDictionaryEntry<TKey, TValue>(pair.Key, pair.Value);
                entries.Add(entry);
            }

            entries.AddRange(invalidEntries);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

#if UNITY_EDITOR
            invalidEntries.Clear();
#endif

            foreach (var entry in entries)
            {
                var key = entry.Key;

                if (key == null || ContainsKey(key))
                {
#if UNITY_EDITOR
                    invalidEntries.Add(entry);
#endif
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
    [Serializable]
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
