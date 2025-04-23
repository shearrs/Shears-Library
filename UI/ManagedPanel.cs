using UnityEngine;
using UnityEngine.Events;

namespace Shears.UI
{
    public class ManagedPanel : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onEnable;
        [SerializeField] private UnityEvent onDisable;

        private bool isEnabled = false;

        public bool IsEnabled => isEnabled;

        protected virtual void Awake()
        {
            if (gameObject.activeSelf)
                Enable();
            else
                Disable();
        }

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

        protected virtual void EnableInternal() { }
        protected virtual void DisableInternal() { }
    }
}
