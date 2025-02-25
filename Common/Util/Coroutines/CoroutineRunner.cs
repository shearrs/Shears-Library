using System.Collections;
using UnityEngine;

namespace Shears.Common
{
    public class CoroutineRunner : PersistentProtectedSingleton<CoroutineRunner>
    {
        public static Coroutine Start(IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        public static void Stop(Coroutine routine)
        {
            Instance.StopCoroutine(routine);
        }
    }
}