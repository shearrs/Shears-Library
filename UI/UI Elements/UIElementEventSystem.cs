using Shears.Input;
using Shears.Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000)]
    public class UIElementEventSystem : MonoBehaviour
    {
        private enum DetectionType { Canvas, World3D }

        private static bool applicationIsQuitting = false;
        private static UIElementEventSystem canvasSystem;

        [SerializeField] private DetectionType detectionType = DetectionType.Canvas;

        private readonly RaycastHit[] results3D = new RaycastHit[10];
        private readonly HashSet<UIElementCanvas> registeredCanvases = new();
        private readonly List<Graphic> hitGraphics = new();
        private LayerMask detectionMask;
        private ManagedInputMap inputMap;
        private IManagedInput clickInput;
        private UIElement hoveredElement;
        private UIElement pointerDownElement;

        [RuntimeInitializeOnLoadMethod()]
        private static void ApplicationRegistration()
        {
            applicationIsQuitting = false;
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            applicationIsQuitting = true;
        }

        private void Awake()
        {
            detectionMask = LayerMask.GetMask("UI");

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>("ManagedElements/Shears_DefaultEventSystemInputMap");

            clickInput = inputMap.GetInput("Click");

            if (detectionType == DetectionType.Canvas)
                canvasSystem = this;
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

        public static void RegisterCanvas(UIElementCanvas canvas)
        {
            if (canvasSystem == null)
            {
                SHLogger.Log($"No canvas system was set! You need to have a {nameof(UIElementEventSystem)} with {nameof(detectionType)} set to {nameof(DetectionType.Canvas)}!", SHLogLevels.Error);
                return;
            }

            canvasSystem.InstRegisterCanvas(canvas);
        }
        private void InstRegisterCanvas(UIElementCanvas canvas)
        {
            if (!registeredCanvases.Contains(canvas))
                registeredCanvases.Add(canvas);
        }

        public static void DeregisterCanvas(UIElementCanvas canvas)
        {
            if (applicationIsQuitting)
                return;

            if (canvasSystem == null)
            {
                SHLogger.Log($"No canvas system was set! You need to have a {nameof(UIElementEventSystem)} with {nameof(detectionType)} set to {nameof(DetectionType.Canvas)}!", SHLogLevels.Error);
                return;
            }

            canvasSystem.InstDeregisterCanvas(canvas);
        }
        private void InstDeregisterCanvas(UIElementCanvas canvas)
        {
            registeredCanvases.Remove(canvas);
        }

        private void UpdateHoveredElement()
        {
            UIElement newHoverTarget = null;

            if (detectionType == DetectionType.Canvas)
                newHoverTarget = RaycastCanvas();
            else if (detectionType == DetectionType.World3D)
                newHoverTarget = Raycast3D();

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverExitEvent());

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverEnterEvent());
        }

        private UIElement RaycastCanvas()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;

            if (pointerPos == Vector2.zero)
                return null;

            hitGraphics.Clear();

            foreach (var canvas in registeredCanvases)
            {
                if (canvas.Raycaster == null)
                    continue;

                var raycastableGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas.UnityCanvas);

                for (int i = 0; i < raycastableGraphics.Count; i++)
                {
                    var graphic = raycastableGraphics[i];

                    if (graphic == null || !graphic.raycastTarget)
                        continue;

                    if (RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPos))
                        hitGraphics.Add(graphic);
                }
            }
            
            int greatestDepth = int.MinValue;
            UIElement element = null;

            foreach (var graphic in hitGraphics)
            {
                if (graphic.depth > greatestDepth && TryGetUIElement(graphic.gameObject, out var hitElement))
                {
                    greatestDepth = graphic.depth;
                    element = hitElement;
                }
            }

            return element;
        }

        private bool TryGetUIElement(GameObject gameObject, out UIElement element)
        {
            if (gameObject.TryGetComponent(out element))
                return true;

            element = gameObject.GetComponentInParent<UIElement>();

            return element != null;
        }

        private UIElement Raycast3D()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(pointerPos);

            int hits = Physics.RaycastNonAlloc(ray, results3D, 1000, detectionMask, QueryTriggerInteraction.Collide);

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
            if (hoveredElement != null)
            {
                hoveredElement.InvokeEvent(new PointerUpEvent());

                if (hoveredElement == pointerDownElement)
                    hoveredElement.InvokeEvent(new ClickEvent());
            }

            pointerDownElement = null;
        }
    }
}
