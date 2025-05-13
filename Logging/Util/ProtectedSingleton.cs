using UnityEngine;

namespace InternProject.Logging
{
    /// <summary>
    /// Allows for a class to be a singleton instance while preventing access to the instance.<br/>
    /// Good for mimicking static class usage while being able to perform instanced actions.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="MonoBehaviour"/> deriving from this.</typeparam>
    [DefaultExecutionOrder(-100)]
    public class ProtectedSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        protected static T Instance
        {
            get
            {
                if (instance == null)
                    instance = FindOrCreateInstance();

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

        private static T FindOrCreateInstance()
        {
            T foundInstance = FindFirstObjectByType<T>();

            if (foundInstance != null)
                return foundInstance;

            GameObject obj = new(typeof(T).Name, typeof(T));
            T component = obj.GetComponent<T>();

            return component;
        }

        public static bool IsInstanceActive() => instance != null;

        public static void CreateInstanceIfNoneExists()
        {
            if (instance == null)
                FindOrCreateInstance();
        }
    }
}
