using System;
using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.DataManagement
{
    public class DataMap
    {
        private readonly Dictionary<Type, object> data = new();

        public int Count => data.Count;

        public DataMap() { }

        public DataMap(params object[] data)
        {
            foreach (var d in data)
                this.data[d.GetType()] = d;
        }

        public void AddData<T>(T data)
        {
            this.data[typeof(T)] = data;
        }

        public bool TryGetData<T>(out T data)
        {
            var type = typeof(T);

            if (this.data.TryGetValue(type, out var objData))
            {
                data = (T)objData;
                return true;
            }

            SHLogger.Log(
                $"{nameof(DataMap)} does not contain type {type.Name}!",
                SHLogLevels.Warning
            );

            data = default;
            return false;
        }
    }
}
