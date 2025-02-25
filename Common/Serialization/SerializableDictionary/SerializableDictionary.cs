using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeReference] private List<TKey> keys = new();
        [SerializeReference] private List<TValue> values = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach(KeyValuePair<TKey, TValue> pair in this)
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
                throw new System.Exception($"There are {keyCount} keys and {valueCount} values after deserialization. Make sure that both key and value types are serializable.");

            for (int i = 0; i < keyCount; i++)
                Add(keys[i], values[i]);
        }
    }
}
