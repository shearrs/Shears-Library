using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Shears.Services
{
    public class ServiceLocator : MonoBehaviour
    {
        private const string GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string SceneServiceLocatorName = "ServiceLocator [Scene]";

        private static ServiceLocator global;
        private static Dictionary<Scene, ServiceLocator> sceneContainers;
        private static List<GameObject> tempSceneGameObjects;

        private readonly ServiceManager services = new();

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (global == this)
                Debug.LogWarning("Already configured as global", this);
            else if (global != null)
                Debug.LogError("Another ServiceLocator is already configured as global", this);
            else
            {
                global = this;

                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (sceneContainers.ContainsKey(scene))
            {
                Debug.LogError("Another ServiceLocator is already configured for this scene", this);
                return;
            }

            sceneContainers.Add(scene, this);
        }

        public static ServiceLocator Global
        {
            get
            {
                if (global != null)
                    return global;

                if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is { } bootstrapper)
                {
                    bootstrapper.BootstrapOnDemand();

                    return global;
                }

                GameObject container = new(GlobalServiceLocatorName, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();

                return global;
            }
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            Scene scene = mb.gameObject.scene;

            if (sceneContainers.TryGetValue(scene, out ServiceLocator locator) && locator != mb)
                return locator;

            tempSceneGameObjects.Clear();

            static bool hasSceneBootstrapper(GameObject go) => go.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper);

            scene.GetRootGameObjects(tempSceneGameObjects);
            foreach (GameObject go in tempSceneGameObjects.Where(go => hasSceneBootstrapper(go)))
            {
                if (go.TryGetComponent(out ServiceLocatorSceneBootstrapper bootstrapper) && bootstrapper.Locator != mb)
                {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Locator;
                }
            }

            return Global;
        }

        public static ServiceLocator For(MonoBehaviour mb)
        {
            return mb.GetComponentInParent<ServiceLocator>()
                     .OrNull() ?? ForSceneOf(mb) ?? Global;
        }

        public ServiceLocator Register<T>(T service)
        {
            services.Register(service);

            return this;
        }

        public ServiceLocator Register(Type type, object service)
        {
            services.Register(type, service);

            return this;
        }

        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service))
                return this;

            if (TryGetNextInHierarchy(out ServiceLocator locator))
            {
                locator.Get(out service);

                return this;
            }

            throw new ArgumentException($"Service of type {typeof(T).FullName} is not registered!");
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            return services.TryGet(out service);
        }

        private bool TryGetNextInHierarchy(out ServiceLocator locator)
        {
            locator = null;

            if (this == global)
                return false;

            locator = transform.parent.OrNull()?
                .GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);

            return locator != null;
        }

        private void OnDestroy()
        {
            if (this == global)
                global = null;
            else if (sceneContainers.ContainsValue(this))
                sceneContainers.Remove(gameObject.scene);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            global = null;
            sceneContainers = new();
            tempSceneGameObjects = new();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/ServiceLocator/Add Global")]
        static void AddGlobal()
        {
            GameObject go = new(GlobalServiceLocatorName, typeof(ServiceLocatorGlobalBootstrapper));
        }

        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        static void AddScene()
        {
            GameObject go = new(SceneServiceLocatorName, typeof(ServiceLocatorSceneBootstrapper));
        }
#endif
    }
}
