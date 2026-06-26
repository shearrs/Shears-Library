using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Logging;
using UnityEngine;

namespace Shears.DataManagement
{
    public abstract class Database<TData, TResult> : ScriptableObject, ISHLoggable
        where TData : DataItem<TResult>
        where TResult : IDataResult<TResult>
    {
        private readonly Dictionary<Type, TData> map = new();
        private readonly Dictionary<string, Type> nameMap = new();
        private readonly List<TData> randomChoices = new();

        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogUtil.Default;

        [Header("Data")]
        [SerializeField]
        private List<TData> data;

        public void Initialize()
        {
            map.Clear();

            foreach (var dataItem in data)
            {
                map[dataItem.DataType] = dataItem;
                nameMap[dataItem.DataType.Name] = dataItem.DataType;
            }
        }

        public TData GetData(Type type)
        {
            if (map.TryGetValue(type, out var data))
                return data;

            this.Log(
                $"{GetType().Name.PascalSpace()} does not contain data for type {type.Name.PascalSpace()}!",
                SHLogLevels.Error
            );

            return default;
        }

        public TData GetDataByName(string name)
        {
            if (nameMap.TryGetValue(name, out var type))
                return GetData(type);

            this.Log(
                $"{GetType().Name.PascalSpace()} does not contain data for name {name}!",
                SHLogLevels.Error
            );

            return default;
        }

        public Type GetTypeByName(string name)
        {
            if (nameMap.TryGetValue(name, out var type))
                return type;

            this.Log(
                $"{GetType().Name.PascalSpace()} does not contain data for name {name}!",
                SHLogLevels.Error
            );

            return default;
        }

        public TData GetRandomData(IReadOnlyCollection<TData> exclusions = null)
        {
            randomChoices.Clear();
            randomChoices.AddRange(map.Values.ToList());

            if (exclusions != null && exclusions.Count > 0)
            {
                foreach (var exclusion in exclusions)
                    randomChoices.Remove(exclusion);
            }

            if (randomChoices.Count == 0)
            {
                this.Log(
                    $"{GetType().Name.PascalSpace()} has no {typeof(TData).Name.PascalSpace()} to choose from!",
                    SHLogLevels.Error
                );
                return null;
            }

            return randomChoices[UnityEngine.Random.Range(0, randomChoices.Count)];
        }
    }
}
