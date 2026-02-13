using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class DataStore : PersistentProtectedSingleton<DataStore>
    {
        private static readonly Dictionary<Type, ScriptableObject> typedData = new();

        [SerializeField]
        private List<ScriptableObject> storedData = new();

        protected override void Awake()
        {
            base.Awake();

            InitializeData();
        }
        
        private void InitializeData()
        {
            foreach (var data in storedData)
                typedData[data.GetType()] = data;
        }

        public static void AddData<T>(T data) where T : ScriptableObject => Instance.InstAddData(data);
        private void InstAddData<T>(T data) where T : ScriptableObject
        {
            storedData.Add(data);
            typedData[data.GetType()] = data;
        }

        public static void RemoveData<T>(T data) where T : ScriptableObject => Instance.InstRemoveData(data);
        private void InstRemoveData<T>(T data) where T : ScriptableObject
        {
            storedData.Remove(data);
            typedData.Remove(data.GetType());
        }

        public static T GetData<T>() where T : ScriptableObject => Instance.InstGetData<T>();
        private T InstGetData<T>() where T : ScriptableObject
        {
            if (typedData.TryGetValue(typeof(T), out var value))
                return (T)value;

            Debug.LogError($"{nameof(DataStore)} has no data for type {typeof(T).Name}!");
            return null;
        }
    }
}
