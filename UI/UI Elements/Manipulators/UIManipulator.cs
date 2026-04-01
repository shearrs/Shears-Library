using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public abstract class UIManipulator : MonoBehaviour
    {
        [Header("UI Manipulator")]
        [SerializeField, RuntimeReadOnly]
        private bool enableOnStart = true;

        private UIElement element;

        private bool isEnabled = false;

        protected UIElement Element => element;
        public bool IsEnabled => isEnabled;

        protected virtual void Awake()
        {
            element = GetComponent<UIElement>();
        }

        protected virtual void Start()
        {
            if (enableOnStart)
                Enable();
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            RegisterEvents();

            isEnabled = true;
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            DeregisterEvents();

            isEnabled = false;
        }

        protected abstract void RegisterEvents();
        protected abstract void DeregisterEvents();
    }
}
