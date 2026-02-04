using System;
using UnityEngine;

namespace Shears
{
    public static class PauseManager
    {
        public static event Action Paused;
        public static event Action Unpaused;

        public static bool IsPaused { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPause()
        {
            IsPaused = false;
        }

        public static void Pause()
        {
            Time.timeScale = 0f;

            IsPaused = true;

            Paused?.Invoke();
        }

        public static void Unpause()
        {
            Time.timeScale = 1f;

            IsPaused = false;

            Unpaused?.Invoke();
        }

        public static void TogglePause()
        {
            if (IsPaused)
                Unpause();
            else
                Pause();
        }
    }
}
