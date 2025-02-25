using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public abstract class ManagedPanel : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onEnable;
        [SerializeField] private UnityEvent onDisable;

        private bool isEnabled = false;

        public bool IsEnabled => isEnabled;

        public void Enable()
        {
            if (isEnabled)
                return;

            gameObject.SetActive(true);
            isEnabled = true;

            EnableInternal();

            onEnable.Invoke();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            gameObject.SetActive(false);
            isEnabled = false;

            DisableInternal();

            onDisable.Invoke();
        }

        protected abstract void EnableInternal();
        protected abstract void DisableInternal();
    }
}
