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
        [SerializeField] private ManagedSelectable selectOnEnable;
        [SerializeField] private ManagedSelectable selectOnDisable;
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

            if (selectOnEnable != null)
                ManagedEventSystem.Select(selectOnEnable);

            OnEnable?.Invoke();
            onEnable.Invoke();
        }

        public void Disable()
        {
            canvas.enabled = false;

            if (selectOnDisable)
                ManagedEventSystem.Select(selectOnDisable);
            else if (clearOnDisable)
                ManagedEventSystem.Select(null);

            OnDisable?.Invoke();
            onDisable.Invoke();
        }
    }
}
