using System.Collections;
using UnityEngine;

namespace Shears
{
    public class CoroutineRunner : PersistentProtectedSingleton<CoroutineRunner>
    {
        public static Coroutine Start(IEnumerator routine)
        {
            if (!CanCreateInstance)
            {
                Debug.LogError("Cannot create CoroutineRunner instance!");
                return null;
            }    

            return Instance.StartCoroutine(routine);
        }

        public static void Stop(Coroutine routine)
        {
            if (!CanCreateInstance)
                return;

            Instance.StopCoroutine(routine);
        }
    }
}