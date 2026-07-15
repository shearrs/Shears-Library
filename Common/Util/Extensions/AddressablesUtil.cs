using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Shears
{
    public static class AddressablesUtil
    {
        public readonly struct LabelGroupData<T>
        {
            public readonly string PrimaryKey { get; }
            public readonly T Result { get; }
            public readonly int Index { get; }

            public LabelGroupData(string primaryKey, T result, int index)
            {
                PrimaryKey = primaryKey;
                Result = result;
                Index = index;
            }
        }

        public static void LoadLabelGroup<T>(
            string label,
            Action<LabelGroupData<T>> processCallback
        )
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));

            locationHandle.WaitForCompletion();

            if (locationHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Failed to load assets with label: {label}!");
                return;
            }

            var locations = locationHandle.Result;
            var opList = ListPool<AsyncOperationHandle>.Get();
            opList.Clear();

            for (int i = 0; i < locations.Count; i++)
            {
                var location = locations[i];
                var handle = Addressables.LoadAssetAsync<T>(location);
                handle.Completed += handleResult =>
                    processCallback(new(location.PrimaryKey, handleResult.Result, i));

                opList.Add(handle);
            }

            var groupHandle = Addressables.ResourceManager.CreateGenericGroupOperation(opList);
            groupHandle.WaitForCompletion();

            locationHandle.Release();
            groupHandle.Release();
        }
    }
}
