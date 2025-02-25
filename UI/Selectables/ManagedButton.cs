using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(ManagedSelectable))]
    public class ManagedButton : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool enableOnAwake = true;
        [SerializeField] private bool clickable = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onClick;

        private ManagedSelectable selectable;

        public bool Clickable { get => clickable; set => clickable = value; }

        private void Awake()
        {
            selectable = GetComponent<ManagedSelectable>();

            if (enableOnAwake)
                Enable();
            else
                Disable();
        }

        public void Enable()
        {
            selectable.Interactable = true;
        }

        public void Disable()
        {
            selectable.Interactable = false;
        }

        public void Click()
        {
            if (!Clickable)
                return;

            onClick.Invoke();
        }
    }
}
