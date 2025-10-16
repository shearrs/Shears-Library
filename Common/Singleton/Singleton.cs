using UnityEngine;

namespace Shears
{
    // Originally inspired by Tarodev on YouTube: https://www.youtube.com/watch?v=tE1qH8OxO2Y

    internal static class SingletonManager
    {
        private static bool canCreateInstances = false;

        public static bool CanCreateInstances => canCreateInstances;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnableInstanceCreation()
        {
            Debug.Log("set true");
            canCreateInstances = true;

            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            canCreateInstances = false;
        }
    }

    /// <summary>
    /// A singleton that hides its instance and is used as a static class. Creates itself if none exists, and destroys itself on <see cref="Awake"/> if another instance already exists.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class ProtectedSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        protected static T Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateInstance();

                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        protected static bool CanCreateInstance => SingletonManager.CanCreateInstances;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();

                GameObject parent = GameObject.Find("Managers");

                if (parent != null)
                    transform.parent = parent.transform;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnApplicationQuit()
        {
            Instance = null;
            Destroy(gameObject);
        }

        private static T CreateInstance()
        {
            if (!CanCreateInstance)
                return null;

            GameObject obj = new(typeof(T).Name, typeof(T));
            T component = obj.GetComponent<T>();

            return component;
        }

        public static bool IsInstanceActive() => instance != null;

        public static void CreateInstanceIfNoneExists()
        {
            if (!CanCreateInstance)
                return;

            if (instance == null)
                instance = FindAnyObjectByType<T>();

            if (instance == null)
            {
                instance = CreateInstance();
                
                var singleton = instance as ProtectedSingleton<T>;
                singleton.OnInstanceCreated();
            }
        }

        protected virtual void OnInstanceCreated()
        {
        }
    }

    /// <summary>
    /// A static instance which creates itself if none exists, and overrides a previous instance on <see cref="Awake"/> (without destroying the previous instance).
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateInstance();

                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();
            }
        }

        private static T CreateInstance()
        {
            GameObject obj = new(typeof(T).Name, typeof(T));
            T component = obj.GetComponent<T>();

            GameObject parent = GameObject.Find("Managers");

            if (parent == null)
            {
                parent = new("Managers");
                parent.transform.SetSiblingIndex(0);
            }

            obj.transform.parent = parent.transform;

            return component;
        }

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// A singleton which creates itself if none exists, and destroys itself on <see cref="Awake"/> if another instance already exists.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            if (instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// A singleton which creates itself if none exists, destroys itself on <see cref="Awake"/> if another instance already exists, and persists through scene loads.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();

                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// A singleton that hides its instance and is used as a static class. Creates itself if none exists, destroys itself on <see cref="Awake"/> if another instance already exists, and persists through scene loads.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class PersistentProtectedSingleton<T> : ProtectedSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (instance == null)
            {
                instance = GetComponent<T>();

                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
