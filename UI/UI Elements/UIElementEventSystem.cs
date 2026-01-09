using Shears.Input;
using Shears.Logging;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000)]
    public partial class UIElementEventSystem : MonoBehaviour
    {
        #region Variables
        const float DRAG_BEGIN_TIME = 0.1f;
        const float DRAG_BEGIN_SQR_DISTANCE = 0.25f * 0.25f;

        public enum DetectionType { Canvas, World3D }

        private static bool applicationIsQuitting = false;
        private static UIElementEventSystem canvasSystem;

        [SerializeField] private DetectionType detectionType = DetectionType.Canvas;

        private readonly RaycastHit[] results3D = new RaycastHit[10];
        private readonly List<RaycastHit> sortedResults = new(10);
        private readonly HashSet<UIElementCanvas> registeredCanvases = new();
        private readonly List<Graphic> hitGraphics = new();

        private ManagedInputMap inputMap;

        [AutoEvent(nameof(IManagedInput.Started), nameof(OnPointerDown))]
        [AutoEvent(nameof(IManagedInput.Canceled), nameof(OnPointerUp))]
        private IManagedInput clickInput;

        private LayerMask detectionMask;
        private UIElement hoveredElement;
        private UIElement draggedElement;
        private UIElement pointerDownElement;
        private float pointerDownTime;
        private Vector2 pointerDownPosition;

        public DetectionType SystemType => detectionType;
        #endregion

        #region Static Initialization
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
        #endregion

        #region Unity Methods
        private void Awake()
        {
            detectionMask = LayerMask.GetMask("UI");

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>("ManagedElements/Shears_DefaultEventSystemInputMap");

            clickInput = inputMap.GetInput("Click");

            if (detectionType == DetectionType.Canvas)
                canvasSystem = this;
        }

        private void Update()
        {
            UpdateHoveredElement();
            UpdateDraggedElement();
        }
        #endregion

        #region Registration
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

            canvasSystem.InstDeregisterCanvas(canvas);
        }
        private void InstDeregisterCanvas(UIElementCanvas canvas)
        {
            registeredCanvases.Remove(canvas);
        }
        #endregion

        private void UpdateHoveredElement()
        {
            UIElement newHoverTarget = null;

            if (detectionType == DetectionType.Canvas)
                newHoverTarget = RaycastCanvas();
            else if (detectionType == DetectionType.World3D)
            {
                if (canvasSystem != null && canvasSystem.hoveredElement != null) // world raycasts are blocked by canvas elements
                    newHoverTarget = null;
                else
                    newHoverTarget = Raycast3D();
            }

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverExitEvent());

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.InvokeEvent(new HoverEnterEvent());
        }

        private void UpdateDraggedElement()
        {
            var camera = Camera.main;
            Vector2 pointerPos = ManagedPointer.Current.Position;

            if (draggedElement == null)
            {
                if (pointerDownElement == null)
                    return;
                else if (Time.time - pointerDownTime < DRAG_BEGIN_TIME)
                    return;

                float sqrDistance = (pointerDownPosition - pointerPos).sqrMagnitude;

                if (sqrDistance < DRAG_BEGIN_SQR_DISTANCE)
                    return;
            }

            var targetElement = (pointerDownElement != null) ? pointerDownElement : draggedElement;

            Vector3 direction = (camera.transform.position - transform.position);
            var planePosition = camera.ScreenPointToPlanePosition(
                ManagedPointer.Current.Position, direction,
                targetElement.transform.position
            );

            Vector3 offset = targetElement.transform.position - planePosition;

            if (draggedElement == null)
            {
                draggedElement = pointerDownElement;
                draggedElement.InvokeEvent(new DragBeginEvent(camera, pointerPos, offset));
            }
            else if (pointerDownElement == null)
            {
                draggedElement.InvokeEvent(new DragEndEvent(camera, pointerPos, planePosition));
                draggedElement = null;

                return;
            }

            draggedElement.InvokeEvent(new DragEvent(camera, pointerPos, planePosition));
        }

        #region Raycasts
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

                    var cam = canvas.UnityCanvas.worldCamera;

                    if (canvas.UnityCanvas.renderMode != RenderMode.ScreenSpaceCamera)
                        cam = null;

                    if (RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPos, cam))
                        hitGraphics.Add(graphic);
                }
            }
            
            Graphic targetGraphic = null;
            UIElement element = null;

            foreach (var graphic in hitGraphics)
            {
                if (targetGraphic == null 
                    || graphic.canvas.renderOrder > targetGraphic.canvas.renderOrder
                    || graphic.canvas.renderOrder == targetGraphic.canvas.renderOrder && graphic.depth > targetGraphic.depth)
                    targetGraphic = graphic;
            }

            if (targetGraphic != null)
                TryGetUIElement(targetGraphic.gameObject, out element);

            return element;
        }

        private UIElement Raycast3D()
        {
            var camera = Camera.main;

            if (camera == null)
            {
                SHLogger.Log($"{nameof(UIElementEventSystem)} requires a MainCamera in the scene to raycast!", SHLogLevels.Error);
                return null;
            }

            Vector2 pointerPos = ManagedPointer.Current.Position;
            var ray = camera.ScreenPointToRay(pointerPos);

            int hits = Physics.RaycastNonAlloc(ray, results3D, 1000, detectionMask, QueryTriggerInteraction.Collide);

            sortedResults.Clear();

            for (int i = 0; i < hits; i++)
                sortedResults.Add(results3D[i]);

            sortedResults.Sort((r1, r2) => r1.distance.CompareTo(r2.distance));

            for (int i = 0; i < hits; i++)
            {
                var hit = sortedResults[i];

                if (TryGetUIElement(hit.collider.gameObject, out var element))
                    return element;
            }

            return null;
        }

        private bool TryGetUIElement(GameObject gameObject, out UIElement element)
        {
            if (gameObject.TryGetComponent(out element))
                return true;

            element = gameObject.GetComponentInParent<UIElement>();

            return element != null;
        }
        #endregion

        private void OnPointerDown()
        {
            if (hoveredElement == null)
                return;

            pointerDownElement = hoveredElement;
            pointerDownElement.InvokeEvent(new PointerDownEvent());

            pointerDownTime = Time.time;
            pointerDownPosition = ManagedPointer.Current.Position;
        }

        private void OnPointerUp()
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
