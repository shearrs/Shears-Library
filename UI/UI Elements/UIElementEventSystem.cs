using Shears.Input;
using Shears.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000)]
    public partial class UIElementEventSystem : MonoBehaviour
    {
        #region Variables
        public enum DetectionType { Canvas, World3D }

        const int MAX_RAYCAST_HITS = 10;
        const float DRAG_BEGIN_SQR_DISTANCE = 0.25f * 0.25f;

        private static readonly RaycastHit[] s_results3D = new RaycastHit[MAX_RAYCAST_HITS];
        private static readonly List<RaycastHit> s_sortedHits = new(MAX_RAYCAST_HITS);
        private static readonly List<UIElement> s_sortedResults = new(MAX_RAYCAST_HITS);
        private static bool applicationIsQuitting = false;
        private static UIElementEventSystem canvasSystem;

        [SerializeField]
        [AutoProperty("SystemType")]
        private DetectionType detectionType = DetectionType.Canvas;

        private readonly RaycastHit[] results3D = new RaycastHit[MAX_RAYCAST_HITS];
        private readonly List<RaycastHit> sortedHits = new(MAX_RAYCAST_HITS);
        private readonly HashSet<UIElementCanvas> registeredCanvases = new();
        private readonly List<Graphic> hitGraphics = new();
        private readonly HashSet<UIElement> registeredElements = new();

        private ManagedInputMap inputMap;

        [AutoEvent(nameof(IManagedInput.Started), nameof(OnPointerDown))]
        [AutoEvent(nameof(IManagedInput.Canceled), nameof(OnPointerUp))]
        private IManagedInput clickInput;

        [AutoEvent(nameof(IManagedInput.Started), nameof(OnSelectDown))]
        [AutoEvent(nameof(IManagedInput.Canceled), nameof(OnSelectUp))]
        private IManagedInput selectInput;

        private static LayerMask detectionMask;
        private UIElement hoveredElement;
        private UIElement draggedElement;
        private UIElement pointerDownElement;
        private UIElement focusedElement;
        private float pointerDownTime;
        private Vector2 pointerDownPosition;
        private float dragInitialZ;
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
            selectInput = inputMap.GetInput("Select");

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
        private void InstDeregisterCanvas(UIElementCanvas canvas) => registeredCanvases.Remove(canvas);

        public static void RegisterElement(UIElement element)
        {
            if (canvasSystem == null)
            {
                SHLogger.Log($"No canvas system was set! You need to have a {nameof(UIElementEventSystem)} with {nameof(detectionType)} set to {nameof(DetectionType.Canvas)}!", SHLogLevels.Error);
                return;
            }

            canvasSystem.InstRegisterElement(element);
        }
        private void InstRegisterElement(UIElement element)
        {
            if (!registeredElements.Contains(element))
                registeredElements.Add(element);
        }

        public static void DeregisterElement(UIElement element) => canvasSystem.InstDeregisterElement(element);
        private void InstDeregisterElement(UIElement element) => registeredElements.Remove(element);
        #endregion

        public static void Focus(UIElement element)
        {
            if (canvasSystem == null)
            {
                SHLogger.Log($"No canvas system was set! You need to have a {nameof(UIElementEventSystem)} with {nameof(detectionType)} set to {nameof(DetectionType.Canvas)}!", SHLogLevels.Error);
                return;
            }

            canvasSystem.InstFocus(element);
        }
        private void InstFocus(UIElement element)
        {
            if (applicationIsQuitting)
                return;

            ClearFocus();

            focusedElement = element;

            if (focusedElement != null)
            {
                focusedElement.InvokeEvent(new FocusEnterEvent());
                focusedElement.Disabled += ClearFocus;
            }
        }

        private void ClearFocus()
        {
            if (applicationIsQuitting || focusedElement == null)
                return;

            focusedElement.InvokeEvent(new FocusExitEvent());
            focusedElement.Disabled -= ClearFocus;
            focusedElement = null;
        }

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
                {
                    Raycast3DInternal(results3D, sortedHits);
                    newHoverTarget = FindFirstUIElement(sortedHits);
                }
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
                else if (Time.time - pointerDownTime < pointerDownElement.DragBeginTime)
                    return;

                float sqrDistance = (pointerDownPosition - pointerPos).sqrMagnitude;

                if (sqrDistance < DRAG_BEGIN_SQR_DISTANCE)
                    return;

                pointerPos = pointerDownPosition;
            }

            var targetElement = (draggedElement != null) ? draggedElement : pointerDownElement;
            Vector3 targetPosition = targetElement.transform.position;

            if (draggedElement != null)
                targetPosition.z = dragInitialZ;

            Vector3 direction = (camera.transform.position - transform.position);
            var planePosition = camera.ScreenPointToPlanePosition(
                pointerPos, direction,
                targetPosition
            );

            Vector3 offset = targetElement.transform.position - planePosition;

            if (draggedElement == null)
            {
                draggedElement = pointerDownElement.GetDeepestChild();
                draggedElement.InvokeEvent(new DragBeginEvent(camera, pointerPos, offset));
                dragInitialZ = targetElement.transform.position.z;
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
        public static void Raycast3D(List<RaycastHit> sortedHits, List<UIElement> hitElements)
        {
            hitElements.Clear();

            Raycast3DInternal(s_results3D, sortedHits);

            for (int i = 0; i < sortedHits.Count; i++)
            {
                var hit = sortedHits[i];

                if (TryGetUIElement(hit.collider.gameObject, out var element))
                    hitElements.Add(element);
            }
        }

        public static bool TryRaycastElement<T>(out T element) where T : UIElement
        {
            Raycast3D(s_sortedHits, s_sortedResults);

            for (int i = 0; i < s_sortedResults.Count; i++)
            {
                var result = s_sortedResults[i];

                if (result is T typedElement)
                {
                    element = typedElement;
                    return true;
                }
            }

            element = null;
            return false;
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

        private static void Raycast3DInternal(RaycastHit[] raycastHits, List<RaycastHit> sortedHits)
        {
            var camera = Camera.main;

            if (camera == null)
            {
                SHLogger.Log($"{nameof(UIElementEventSystem)} requires a MainCamera in the scene to raycast!", SHLogLevels.Error);
                return;
            }

            Vector2 pointerPos = ManagedPointer.Current.Position;
            var ray = camera.ScreenPointToRay(pointerPos);

            int hits = Physics.RaycastNonAlloc(ray, raycastHits, 1000, detectionMask, QueryTriggerInteraction.Collide);

            sortedHits.Clear();

            for (int i = 0; i < hits; i++)
                sortedHits.Add(raycastHits[i]);

            sortedHits.Sort((r1, r2) => r1.distance.CompareTo(r2.distance));
        }

        private UIElement FindFirstUIElement(List<RaycastHit> hits)
        {
            for (int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];

                if (TryGetUIElement(hit.collider.gameObject, out var element))
                    return element;
            }

            return null;
        }

        private static bool TryGetUIElement(GameObject gameObject, out UIElement element)
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

        private void OnSelectDown()
        {
            if (focusedElement != null)
                focusedElement.InvokeEvent(new PointerDownEvent());
        }

        private void OnSelectUp()
        {
            if (focusedElement != null)
            {
                focusedElement.InvokeEvent(new PointerUpEvent());
                focusedElement.InvokeEvent(new ClickEvent());
            }
        }
    }
}
