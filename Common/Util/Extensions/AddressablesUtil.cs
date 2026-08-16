using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        public static async Task LoadLabelGroup<T>(
            string label,
            Action<LabelGroupData<T>> processCallback
        )
        {
            var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));

            await locationHandle.Task;

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

                int id = i;
                handle.Completed += handleResult =>
                    processCallback(new(location.PrimaryKey, handleResult.Result, id));

                opList.Add(handle);
            }

            var groupHandle = Addressables.ResourceManager.CreateGenericGroupOperation(opList);
            await groupHandle.Task;

            locationHandle.Release();
            groupHandle.Release();
        }
    }
}
