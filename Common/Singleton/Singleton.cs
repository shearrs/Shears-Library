using UnityEngine;

namespace Shears
{
    public abstract class SingletonBase : MonoBehaviour
    {
        protected static bool CanCreateInstance { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeStaticState()
        {
            CanCreateInstance = true;
        }
    }

    // Originally inspired by Tarodev on YouTube: https://www.youtube.com/watch?v=tE1qH8OxO2Y
    /// <summary>
    /// A singleton that hides its instance and is used as a static class. Creates itself if none exists, and destroys itself on <see cref="Awake"/> if another instance already exists.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class ProtectedSingleton<T> : SingletonBase where T : SingletonBase
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
            CanCreateInstance = false;

            Instance = null;
            Destroy(gameObject);
        }

        private static T CreateInstance()
        {
            if (!CanCreateInstance)
                return null;

            GameObject obj = new(typeof(T).Name, typeof(T));
            T component = obj.GetComponent<T>();

            // Use this for debugging singleton creation
            Debug.Log($"Created singleton: {typeof(T).Name}.");

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
    /// A singleton that hides its instance and is used as a static class. Creates itself if none exists, destroys itself on <see cref="Awake"/> if another instance already exists, and persists through scene loads.
    /// </summary>
    /// <typeparam name="T">The type of class that inherits from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public abstract class PersistentProtectedSingleton<T> : ProtectedSingleton<T> where T : SingletonBase
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
