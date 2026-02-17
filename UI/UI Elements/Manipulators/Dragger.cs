using System;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public partial class Dragger : UIManipulator
    {
        [Header("Dragger")]
        [SerializeField, RuntimeReadOnly]
        private bool isDraggable = true;

        [SerializeField, RuntimeReadOnly, Min(0.0f)]
        private float dragBeginTime = 0.05f;

        private Vector3 offset;

        public event Action DragBegan;
        public event Action DragEnded;

        protected override void RegisterEvents()
        {
            Element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            Element.RegisterEvent<DragEvent>(OnDrag);
            Element.RegisterEvent<DragEndEvent>(OnDragEnd);
        }

        protected override void DeregisterEvents()
        {
            Element.DeregisterEvent<DragBeginEvent>(OnDragBegin);
            Element.DeregisterEvent<DragEvent>(OnDrag);
            Element.DeregisterEvent<DragEndEvent>(OnDragEnd);
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            evt.PreventBubbleUp();

            offset = evt.PointerWorldOffset;

            DragBegan?.Invoke();
        }

        private void OnDrag(DragEvent evt)
        {
            evt.PreventBubbleUp();

            const float MOVE_SPEED = 8.0f;

            Vector3 pointerWorld = evt.PointerWorldPosition;
            Vector3 targetPosition = pointerWorld + offset;
            Element.transform.position = Vector3.MoveTowards(Element.transform.position, targetPosition, MOVE_SPEED * Time.deltaTime);
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            evt.PreventBubbleUp();

            DragEnded?.Invoke();
        }
    }
}
