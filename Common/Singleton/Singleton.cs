using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Common
{
    [DefaultExecutionOrder(-100)]
    public class ProtectedSingleton<T> : MonoBehaviour where T :MonoBehaviour
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
            Instance = null;
            Destroy(gameObject);
        }

        private static T CreateInstance()
        {
            GameObject obj = new(typeof(T).Name, typeof(T));
            T component = obj.GetComponent<T>();

            return component;
        }

        public static bool IsInstanceActive() => instance != null;
    }

    // overrides instance with new instances rather than destroying them
    [DefaultExecutionOrder(-100)]
    public class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
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

    // destroys any new versions created in favor of the initial instance
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

    // survives through scene loads
    // good for system classes which require persistent data
    // good for audio sources where music plays through loading screens
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