using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ManagedCanvas : ManagedWrapper<Canvas>
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

        private void OnValidate()
        {
            var canvas = GetComponent<Canvas>();

            if (canvas.enabled != enabled)
                StartCoroutine(IEToggleCanvas(canvas));
        }

        protected override void OnDestroy()
        {
            if (TryGetComponent(out CanvasScaler scaler))
                Destroy(scaler);

            if (TryGetComponent(out GraphicRaycaster raycaster))
                Destroy(raycaster);

            base.OnDestroy();
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

        private IEnumerator IEToggleCanvas(Canvas canvas)
        {
            yield return new WaitForEndOfFrame();
            canvas.enabled = enabled;

#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
        }
    }
}
