using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ManagedCanvas : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField] private ManagedUIElement focusOnEnable;
        [SerializeField] private ManagedUIElement focusOnDisable;
        [SerializeField] private bool clearOnDisable = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onEnable;
        [SerializeField] private UnityEvent onDisable;

        private Canvas canvas;

        public event Action OnEnable;
        public event Action OnDisable;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            if (canvas.enabled)
                Enable();
        }

        public void Enable()
        {
            canvas.enabled = true;

            if (focusOnEnable != null)
                ManagedUIEventSystem.Focus(focusOnEnable);

            OnEnable?.Invoke();
            onEnable.Invoke();
        }

        public void Disable()
        {
            canvas.enabled = false;

            if (focusOnDisable)
                ManagedUIEventSystem.Focus(focusOnDisable);
            else if (clearOnDisable)
                ManagedUIEventSystem.Focus(null);

            OnDisable?.Invoke();
            onDisable.Invoke();
        }
    }
}
