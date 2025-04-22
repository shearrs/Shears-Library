using Shears.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Shears.Loading
{
    public class Loader : PersistentProtectedSingleton<Loader>
    {
        [SerializeField] private LoadingScreen loadingScreen;

        private readonly Dictionary<string, SceneInstance> loadedScenes = new();
        private readonly Queue<LoadRequest> loadRequests = new();
        private readonly Queue<Coroutine> loadDelays = new();

        private bool processingRequest = false;
        private bool delaying = false;

        public static bool IsLoading => Instance.processingRequest;
        public static bool IsDelaying { get => Instance.delaying; private set => Instance.delaying = value; }

        public static event Action OnLoadStart;
        public static event Action OnLoadPreComplete;
        public static event Action OnLoadComplete;

        public static void EnqueueRequest(LoadRequest request) => Instance.InstEnqueueRequest(request);
        private void InstEnqueueRequest(LoadRequest request)
        {
            loadRequests.Enqueue(request);

            if (!processingRequest)
                StartCoroutine(IEProcessRequests());
        }

        public static void EnqueueLoadDelay(Coroutine delay) => Instance.InstEnqueueLoadDelay(delay);
        private void InstEnqueueLoadDelay(Coroutine delay)
        {
            loadDelays.Enqueue(delay);
        }

        private IEnumerator IEProcessRequests()
        {
            LoadRequest request = loadRequests.Dequeue();

            processingRequest = true;

            if (request.PausesGame)
                Time.timeScale = 0;

            // if loadingScreen, open it
            if (request.OpensLoadingScreen)
                yield return loadingScreen.Enable();

            OnLoadStart?.Invoke();

            foreach (var action in request.Actions)
            {
                if (action.Type == LoadAction.LoadType.Load)
                    yield return StartCoroutine(IELoad(action));
                else
                    yield return StartCoroutine(IEUnload(action));
            }

            if (request.PausesGame)
                Time.timeScale = 1;

            yield return StartCoroutine(IEProcessDelays(request));

            if (loadRequests.Count > 0)
                StartCoroutine(IEProcessRequests());
            else
            {
                processingRequest = false;
                OnLoadComplete?.Invoke();
            }
        }

        private IEnumerator IEProcessDelays(LoadRequest request)
        {
            IsDelaying = true;
            OnLoadPreComplete?.Invoke();

            while (loadDelays.Count > 0)
            {
                Coroutine delay = loadDelays.Dequeue();

                yield return delay;
            }

            // if loading screen, close it
            if (request.OpensLoadingScreen)
            {
                while (loadingScreen.IsDelaying)
                    yield return null;

                yield return loadingScreen.Disable();
            }

            IsDelaying = false;
        }

        private IEnumerator IELoad(LoadAction action)
        {
            if (loadedScenes.ContainsKey(action.SceneAddress))
            {
                Debug.LogWarning($"Scene [{action.SceneAddress}] already loaded!");

                yield break;
            }

            var asyncLoad = Addressables.LoadSceneAsync(action.SceneAddress, action.Mode);

            yield return asyncLoad;

            if (!asyncLoad.IsValid())
            {
                Debug.LogError($"Failed to load scene: {action.SceneAddress}!");
                yield break;
            }

            if (action.Mode == LoadSceneMode.Single)
                loadedScenes.Clear();

            loadedScenes.Add(action.SceneAddress, asyncLoad.Result);
        }

        private IEnumerator IEUnload(LoadAction action)
        {
            if (!loadedScenes.TryGetValue(action.SceneAddress, out SceneInstance scene))
            {
                Debug.LogWarning($"Cannot unload scene [{action.SceneAddress}]!");

                yield break;
            }

            var asyncLoad = Addressables.UnloadSceneAsync(scene);

            yield return asyncLoad;

            if (!asyncLoad.IsValid())
            {
                Debug.LogError($"Failed to unload scene: {action.SceneAddress}!");
                yield break;
            }

            loadedScenes.Remove(action.SceneAddress);
        }
    }
}
