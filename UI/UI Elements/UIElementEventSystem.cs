using Shears.Input;
using Shears.Logging;
using UnityEngine;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000)]
    public class UIElementEventSystem : ProtectedSingleton<UIElementEventSystem>
    {
        private readonly RaycastHit[] results3D = new RaycastHit[10];
        private LayerMask detectionMask;

        private ManagedInputMap inputMap;
        private IManagedInput clickInput;
        private UIElement hoveredElement;
        private UIElement pointerDownElement;

        protected override void Awake()
        {
            base.Awake();

            detectionMask = LayerMask.GetMask("UI");

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>("ManagedElements/Shears_DefaultEventSystemInputMap");

            clickInput = inputMap.GetInput("Click");
        }

        private void OnEnable()
        {
            clickInput.Started += OnPointerDown;
            clickInput.Canceled += OnPointerUp;
        }

        private void OnDisable()
        {
            clickInput.Started -= OnPointerDown;
            clickInput.Canceled -= OnPointerUp;
        }

        private void Update()
        {
            UpdateHoveredElement();
        }

        private void UpdateHoveredElement()
        {
            var newHoverTarget = Raycast3D();

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverExitEvent());

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverEnterEvent());
        }

        private UIElement Raycast3D()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(pointerPos);

            int hits = Physics.RaycastNonAlloc(ray, results3D, 1000, detectionMask, QueryTriggerInteraction.Collide);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

            for (int i = 0; i < hits; i++)
            {
                var hit = results3D[i];

                if (hit.collider.TryGetComponent<UIElement>(out var element))
                    return element;
            }

            return null;
        }
    
        private void OnPointerDown(ManagedInputInfo info)
        {
            if (hoveredElement == null)
                return;

            pointerDownElement = hoveredElement;
            pointerDownElement.InvokeEvent(new PointerDownEvent());
        }

        private void OnPointerUp(ManagedInputInfo info)
        {
            if (pointerDownElement == null)
                return;
            else if (hoveredElement == pointerDownElement)
                pointerDownElement.InvokeEvent(new PointerUpEvent());

            pointerDownElement = null;
        }
    }
}
