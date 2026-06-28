using System;
using System.Collections.Generic;
using Shears.Input;
using Shears.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000)]
    public partial class UIElementEventSystem : PersistentProtectedSingleton<UIElementEventSystem>
    {
        #region Variables
        [Flags]
        public enum DetectionTypes
        {
            Canvas = 1 << 0,
            World3D = 1 << 1,
        }

        const int MAX_RAYCAST_HITS = 10;
        const float DRAG_BEGIN_SQR_DISTANCE = 0.01f * 0.01f;

        [SerializeField]
        [AutoProperty("SystemType")]
        private DetectionTypes detectionTypes = (DetectionTypes)(-1);

        [AutoEvent(nameof(IManagedInput.Started), nameof(OnPointerDown))]
        [AutoEvent(nameof(IManagedInput.Canceled), nameof(OnPointerUp))]
        private IManagedInput clickInput;

        [AutoEvent(nameof(IManagedInput.Started), nameof(OnSelectDown))]
        [AutoEvent(nameof(IManagedInput.Canceled), nameof(OnSelectUp))]
        private IManagedInput selectInput;

        private static readonly RaycastHit[] results3D = new RaycastHit[MAX_RAYCAST_HITS];
        private static readonly List<RaycastHit> sortedHits = new(MAX_RAYCAST_HITS);
        private static readonly List<UIElement> sortedResults = new(MAX_RAYCAST_HITS);
        private static readonly HashSet<UIElementCanvas> registeredCanvases = new();
        private static readonly HashSet<UIElement> registeredElements = new();
        private static readonly List<UIElement> hoveredElements = new();
        private static readonly List<UIElement> newHoveredElements = new();
        private static readonly List<Graphic> hitGraphics = new();
        private static bool applicationIsQuitting = false;
        private static ManagedInputMap inputMap;
        private static LayerMask detectionMask;
        private static UIElement hoveredElement;
        private static UIElement draggedElement;
        private static UIElement pointerDownElement;
        private static UIElement focusedElement;
        private static bool hoveredCanvasTarget;
        private static float pointerDownTime;
        private static Vector2 pointerDownPosition;
        private static float dragInitialZ;

        public static bool IsHovering => hoveredElement != null;
        public static bool IsHoveringCanvasTarget => IsHovering && hoveredCanvasTarget;
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
        protected override void Awake()
        {
            base.Awake();
            registeredCanvases.Clear();
            registeredElements.Clear();

            detectionMask = LayerMask.GetMask("UI");

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>(
                    "ManagedElements/Shears_DefaultEventSystemInputMap"
                );

            clickInput = inputMap.GetInput("Click");
            selectInput = inputMap.GetInput("Select");
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
            if (!registeredCanvases.Contains(canvas))
                registeredCanvases.Add(canvas);
        }

        public static void DeregisterCanvas(UIElementCanvas canvas)
        {
            registeredCanvases.Remove(canvas);
        }

        public static void RegisterElement(UIElement element)
        {
            if (!registeredElements.Contains(element))
                registeredElements.Add(element);
        }

        public static void DeregisterElement(UIElement element) =>
            registeredElements.Remove(element);
        #endregion

        public static void Focus(UIElement element)
        {
            if (applicationIsQuitting)
                return;

            ClearFocus();

            focusedElement = element;
            element.IsFocused = true;

            if (focusedElement != null)
            {
                focusedElement.InvokeEvent(new FocusEnterEvent());
                focusedElement.Disabled += ClearFocus;
            }
        }

        private static void ClearFocus()
        {
            if (applicationIsQuitting || focusedElement == null)
                return;

            focusedElement.IsFocused = false;
            focusedElement.Disabled -= ClearFocus;
            focusedElement.InvokeEvent(new FocusExitEvent());
            focusedElement = null;
        }

        private void UpdateHoveredElement()
        {
            if (draggedElement != null)
            {
                if (hoveredElement == draggedElement)
                    return;
                else if (hoveredElement != null)
                    hoveredElement.InvokeEvent(new HoverExitEvent());

                hoveredElement = draggedElement;
                hoveredElement.InvokeEvent(new HoverEnterEvent());
                return;
            }

            UIElement canvasTarget = null;
            UIElement target3D = null;
            UIElement newHoverTarget = null;

            if ((detectionTypes & DetectionTypes.Canvas) != 0)
                canvasTarget = RaycastCanvas();
            if ((detectionTypes & DetectionTypes.World3D) != 0)
            {
                Raycast3DInternal(results3D, sortedHits);
                target3D = FindFirstUIElement(sortedHits);
            }

            hoveredCanvasTarget = false;

            if (canvasTarget == null && target3D == null)
                newHoverTarget = null;
            else if (canvasTarget != null && target3D == null)
            {
                newHoverTarget = canvasTarget;
                hoveredCanvasTarget = true;
            }
            else if (canvasTarget == null && target3D != null)
                newHoverTarget = target3D;
            else
            {
                if (target3D.IsChildOfCanvas())
                {
                    if (canvasTarget.Canvas.SortOrder > target3D.Canvas.SortOrder)
                        newHoverTarget = canvasTarget;
                    else if (target3D.Canvas.SortOrder > canvasTarget.Canvas.SortOrder)
                        newHoverTarget = target3D;
                    else if (canvasTarget.SortOrder > target3D.SortOrder)
                        newHoverTarget = canvasTarget;
                    else
                        newHoverTarget = target3D;

                    hoveredCanvasTarget = true;
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
                pointerPos,
                direction,
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

            Raycast3DInternal(results3D, sortedHits);

            for (int i = 0; i < sortedHits.Count; i++)
            {
                var hit = sortedHits[i];

                if (TryGetUIElement(hit.collider.gameObject, out var element))
                    hitElements.Add(element);
            }
        }

        public static bool TryRaycastElement<T>(out T component)
            where T : Component
        {
            Raycast3D(sortedHits, sortedResults);

            for (int i = 0; i < sortedResults.Count; i++)
            {
                var result = sortedResults[i];

                if (result.TryGetComponent(out T typedComponent))
                {
                    component = typedComponent;
                    return true;
                }
            }

            component = null;
            return false;
        }

        private UIElement RaycastCanvas()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;

            if (
                pointerPos == Vector2.zero
                || float.IsNaN(pointerPos.x)
                || float.IsNaN(pointerPos.y)
            )
                return null;

            hitGraphics.Clear();

            foreach (var canvas in registeredCanvases)
            {
                if (canvas.Raycaster == null)
                    continue;

                var raycastableGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(
                    canvas.UnityCanvas
                );

                for (int i = 0; i < raycastableGraphics.Count; i++)
                {
                    var graphic = raycastableGraphics[i];

                    if (graphic == null || !graphic.raycastTarget)
                        continue;

                    var cam = canvas.UnityCanvas.worldCamera;

                    if (canvas.UnityCanvas.renderMode != RenderMode.ScreenSpaceCamera)
                        cam = null;

                    if (
                        RectTransformUtility.RectangleContainsScreenPoint(
                            graphic.rectTransform,
                            pointerPos,
                            cam
                        )
                    )
                        hitGraphics.Add(graphic);
                }
            }

            Graphic targetGraphic = null;
            UIElement element = null;

            foreach (var graphic in hitGraphics)
            {
                if (
                    targetGraphic == null
                    || graphic.canvas.renderOrder > targetGraphic.canvas.renderOrder
                    || graphic.canvas.renderOrder == targetGraphic.canvas.renderOrder
                        && graphic.depth > targetGraphic.depth
                )
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
                SHLogger.Log(
                    $"{nameof(UIElementEventSystem)} requires a MainCamera in the scene to raycast!",
                    SHLogLevels.Error
                );
                Instance.gameObject.SetActive(false);
                return;
            }

            Vector2 pointerPos = ManagedPointer.Current.Position;

            if (float.IsNaN(pointerPos.x) || float.IsNaN(pointerPos.y))
            {
                sortedHits.Clear();
                return;
            }

            var ray = camera.ScreenPointToRay(pointerPos);

            int hits = Physics.RaycastNonAlloc(
                ray,
                raycastHits,
                1000,
                detectionMask,
                QueryTriggerInteraction.Collide
            );

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
