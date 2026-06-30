using System;
using UnityEngine;

namespace Shears.DataManagement
{
    public interface IDataItem
    {
        public Type DataType { get; }
    }

    public abstract class DataItem<T> : ScriptableObject, IDataItem
        where T : IDataResult<T>
    {
        [SerializeReference, HideInInspector]
        private T blueprint;

        [SerializeField, HideInInspector]
        private SerializableType selectedType;

        public Type DataType => selectedType;

        public T CreateInstance(DataMap data) => blueprint.Create(data);
    }
}
