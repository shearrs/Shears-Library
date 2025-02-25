using System;
using UnityEngine;

namespace Shears.Common
{
    public class Pauser : MonoBehaviour
    {
        public event Action OnPause;
        public event Action OnUnpause;

        public bool IsPaused { get; private set; }

        public void Pause()
        {
            Time.timeScale = 0f;

            IsPaused = true;

            OnPause?.Invoke();
        }

        public void Unpause()
        {
            Time.timeScale = 1f;

            IsPaused = false;

            OnUnpause?.Invoke();
        }
    }
}
