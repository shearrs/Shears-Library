using System;
using System.Collections.Generic;
using System.Linq;
using Shears.Input;
using Shears.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
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
        private static readonly List<Graphic> hitGraphics = new();
        private static readonly List<Transform> ignoreTargets = new();
        private static readonly List<UIElement> hoveredElements = new();
        private static readonly List<UIElement> newHoveredElements = new();
        private static bool applicationIsQuitting = false;
        private static ManagedInputMap inputMap;
        private static LayerMask detectionMask;
        private static UIElement draggedElement;
        private static UIElement pointerDownElement;
        private static UIElement focusedElement;
        private static bool hoveredCanvasTarget;
        private static float pointerDownTime;
        private static Vector2 pointerDownPosition;
        private static float dragInitialZ;

        private static UIElement HoveredElement =>
            hoveredElements.Count > 0 ? hoveredElements[0] : null;
        public static bool IsHovering => HoveredElement != null;
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

        protected override void OnInstanceCreated()
        {
            Instance.detectionTypes = (DetectionTypes)(-1);
        }
        #endregion

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();
            registeredCanvases.Clear();
            hoveredElements.Clear();
            newHoveredElements.Clear();
            ignoreTargets.Clear();
            hitGraphics.Clear();

            detectionMask = LayerMask.GetMask("UI");

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>(
                    "UIElements/Shears_DefaultEventSystemInputMap"
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
                InvokeEvent(new FocusEnterEvent(), focusedElement);
                focusedElement.Disabled += ClearFocus;
            }
        }

        public static void SetDraggedElement(UIElement overrideElement)
        {
            draggedElement = overrideElement;
        }

        private static void ClearFocus()
        {
            if (applicationIsQuitting || focusedElement == null)
                return;

            focusedElement.IsFocused = false;
            focusedElement.Disabled -= ClearFocus;
            InvokeEvent(new FocusExitEvent(), focusedElement);
            focusedElement = null;
        }

        private void UpdateHoveredElement()
        {
            if (draggedElement != null)
            {
                if (HoveredElement == draggedElement)
                    return;
                else if (HoveredElement != null)
                {
                    InvokeEvent(new HoverExitEvent(), HoveredElement, draggedElement);

                    var index = hoveredElements.IndexOf(draggedElement);
                    if (index != -1)
                        hoveredElements.RemoveRange(index, hoveredElements.Count - index);
                    else
                        hoveredElements.Clear();
                }

                if (hoveredElements.Count == 0)
                {
                    var current = draggedElement;
                    while (current != null)
                    {
                        hoveredElements.Add(current);
                        current = current.Parent;
                    }

                    InvokeEvent(new HoverEnterEvent(), HoveredElement);
                }

                return;
            }

            UIElement canvasTarget = null;
            UIElement target3D = null;
            UIElement newHoverTarget;

            if ((detectionTypes & DetectionTypes.Canvas) != 0)
                canvasTarget = RaycastCanvas();
            if ((detectionTypes & DetectionTypes.World3D) != 0)
            {
                Raycast3DInternal(results3D, sortedHits);
                target3D = FindFirstUIElement(sortedHits);
            }

            if (canvasTarget == null && target3D == null)
                newHoverTarget = null;
            else if (canvasTarget != null && target3D == null)
                newHoverTarget = canvasTarget;
            else if (canvasTarget == null && target3D != null)
                newHoverTarget = target3D;
            else if (target3D.HasCanvasParent)
            {
                if (canvasTarget.RootSortOrder > target3D.RootSortOrder)
                    newHoverTarget = canvasTarget;
                else if (canvasTarget.RootSortOrder < target3D.RootSortOrder)
                    newHoverTarget = target3D;
                else if (canvasTarget.SortOrder > target3D.SortOrder)
                    newHoverTarget = canvasTarget;
                else
                    newHoverTarget = target3D;
            }
            else
                newHoverTarget = canvasTarget;

            if (newHoverTarget != null && newHoverTarget == canvasTarget)
                hoveredCanvasTarget = true;
            else
                hoveredCanvasTarget = false;

            if (newHoverTarget == HoveredElement)
                return;

            newHoveredElements.Clear();

            var element = newHoverTarget;

            while (element != null)
            {
                newHoveredElements.Add(element);
                element = element.Parent;
            }

            int removeIndex = -1;
            int addIndex = newHoveredElements.Count - 1;
            int maxCount = Mathf.Max(hoveredElements.Count, newHoveredElements.Count);

            for (int i = 1; i <= maxCount; i++)
            {
                var currentIndex = hoveredElements.Count - i;
                var newIndex = newHoveredElements.Count - i;

                if (currentIndex < 0)
                {
                    removeIndex = -1;
                    addIndex = newIndex;
                    break;
                }
                else if (newIndex < 0)
                {
                    removeIndex = hoveredElements.Count - 1;
                    addIndex = -1;
                    break;
                }
                else if (hoveredElements[currentIndex] != newHoveredElements[newIndex])
                {
                    removeIndex = currentIndex;
                    addIndex = newIndex;
                    break;
                }
            }

            if (removeIndex != -1)
            {
                if (hoveredElements.Count > removeIndex + 1)
                    InvokeEvent(
                        new HoverExitEvent(),
                        HoveredElement,
                        hoveredElements[removeIndex + 1]
                    );
                else
                    InvokeEvent(new HoverExitEvent(), HoveredElement);

                hoveredElements.RemoveRange(0, removeIndex + 1);
            }

            if (addIndex != -1)
            {
                for (int i = 0; i <= addIndex; i++)
                {
                    var newElement = newHoveredElements[i];
                    hoveredElements.Insert(i, newElement);
                }

                if (hoveredElements.Count > addIndex + 1)
                    InvokeEvent(
                        new HoverEnterEvent(),
                        HoveredElement,
                        hoveredElements[addIndex + 1]
                    );
                else
                    InvokeEvent(new HoverEnterEvent(), HoveredElement);
            }
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
                var possibleTarget = pointerDownElement.GetDeepestChild();
                InvokeEvent(new DragBeginEvent(camera, pointerPos, offset), possibleTarget);
                dragInitialZ = targetElement.transform.position.z;
            }
            else if (pointerDownElement == null)
            {
                InvokeEvent(new DragEndEvent(camera, pointerPos, planePosition), draggedElement);
                ignoreTargets.Clear();

                var children = draggedElement.Children;
                ignoreTargets.Add(draggedElement.transform);

                foreach (var child in children)
                    ignoreTargets.Add(child.transform);

                if (TryRaycastElement(out UIElement releaseTarget, ignoreTargets))
                {
                    InvokeEvent(
                        new DragReleaseTargetEvent(pointerPos, planePosition, draggedElement),
                        releaseTarget
                    );
                }

                InvokeEvent(
                    new DragReleaseEvent(pointerPos, planePosition, releaseTarget),
                    draggedElement
                );

                draggedElement = null;

                return;
            }

            if (draggedElement != null)
                InvokeEvent(new DragEvent(camera, pointerPos, planePosition), draggedElement);
        }

        #region Raycasts
        public static void Raycast3D(
            List<RaycastHit> sortedHits,
            List<UIElement> hitElements,
            IReadOnlyList<Transform> ignoreTargets
        )
        {
            hitElements.Clear();

            Raycast3DInternal(results3D, sortedHits);

            for (int i = 0; i < sortedHits.Count; i++)
            {
                var hit = sortedHits[i];

                if (ignoreTargets != null && ignoreTargets.Contains(hit.transform))
                    continue;

                if (TryGetUIElement(hit.collider.gameObject, out var element))
                    hitElements.Add(element);
            }
        }

        public static bool TryRaycastElement<T>(
            out T component,
            IReadOnlyList<Transform> ignoreTargets = null
        )
            where T : Component
        {
            Raycast3D(sortedHits, sortedResults, ignoreTargets);

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
            sortedHits.Clear();
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

            Debug.DrawRay(ray.origin, ray.direction, Color.blue);

            int hits = Physics.RaycastNonAlloc(
                ray,
                raycastHits,
                1000,
                detectionMask,
                QueryTriggerInteraction.Collide
            );

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
            if (HoveredElement == null)
                return;

            pointerDownElement = HoveredElement;
            InvokeEvent(new PointerDownEvent(), pointerDownElement);

            pointerDownTime = Time.time;
            pointerDownPosition = ManagedPointer.Current.Position;
        }

        private void OnPointerUp()
        {
            if (HoveredElement != null)
            {
                InvokeEvent(new PointerUpEvent(), HoveredElement);

                if (HoveredElement == pointerDownElement)
                    InvokeEvent(new ClickEvent(), HoveredElement);
            }

            pointerDownElement = null;
        }

        private void OnSelectDown()
        {
            if (focusedElement != null)
                InvokeEvent(new PointerDownEvent(), focusedElement);
        }

        private void OnSelectUp()
        {
            if (focusedElement != null)
            {
                InvokeEvent(new PointerUpEvent(), focusedElement);
                InvokeEvent(new ClickEvent(), focusedElement);
            }
        }

        private static void InvokeEvent<EventType>(
            EventType evt,
            UIElement element,
            UIElement boundElement = null
        )
            where EventType : UIEvent
        {
            if (element == boundElement)
                return;

            element.InvokeEvent(evt);

            if (evt.TrickleDown)
            {
                evt.IsTricklingDown = true;
                var children = element.Children;

                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] == boundElement)
                        break;

                    children[i].InvokeEvent(evt);

                    if (!evt.TrickleDown)
                        break;
                }

                evt.IsTricklingDown = false;
            }

            if (evt.BubbleUp)
            {
                evt.IsBubblingUp = true;

                var parent = element.Parent;
                while (parent != null && parent != boundElement)
                {
                    parent.InvokeEvent(evt);
                    parent = parent.Parent;

                    if (!evt.BubbleUp)
                        break;
                }

                evt.IsBubblingUp = false;
            }
        }
    }
}
