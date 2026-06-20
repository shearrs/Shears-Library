using UnityEngine;

namespace Shears
{
    public static class ApplicationUtil
    {
        public static bool IsQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            IsQuitting = false;

            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            IsQuitting = true;
        }
    }
}
