using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public partial class CellContent : MonoBehaviour
    {
        [SerializeField]
        private new Renderer renderer;

        [SerializeField, RuntimeReadOnly]
        private bool isDraggable = true;

        [SerializeField, RuntimeReadOnly, Min(0.0f)]
        private float dragBeginTime = 0.1f;

        [SerializeField]
        private ColorModulator colorModulator;

        [Auto]
        private UIElement element;
        private Vector3 offset;
        private bool initialized = false;

        private void Awake()
        {
            __AutoAwake();

            element.DragBeginTime = dragBeginTime;
        }

        private void Start()
        {
            colorModulator = new(element, renderer.material);

            if (isDraggable)
                EnableDrag();

            initialized = true;
        }

        public void EnableDrag()
        {
            if (isDraggable && initialized)
                return;

            element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            element.RegisterEvent<DragEvent>(OnDrag);
            element.RegisterEvent<DragEndEvent>(OnDragEnd);

            isDraggable = true;
        }

        public void DisableDrag()
        {
            if (!isDraggable)
                return;

            element.DeregisterEvent<DragBeginEvent>(OnDragBegin);
            element.DeregisterEvent<DragEvent>(OnDrag);
            element.DeregisterEvent<DragEndEvent>(OnDragEnd);

            isDraggable = false;
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            colorModulator.TweenToPressed();
            colorModulator.CanChangeColor = false;

            offset = evt.PointerWorldOffset;
        }

        private void OnDrag(DragEvent evt)
        {
            const float MOVE_SPEED = 8.0f;

            Vector3 targetPosition = evt.PointerWorldPosition + offset;
            element.transform.position = Vector3.MoveTowards(element.transform.position, targetPosition, MOVE_SPEED * Time.deltaTime);
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            colorModulator.CanChangeColor = true;

            if (colorModulator.IsHovered)
                colorModulator.TweenToHover();
            else
                colorModulator.ClearModulation();

            if (UIElementEventSystem.TryRaycastElement(out ElementCell cell))
                cell.SetContent(this);
        }
    }
}
