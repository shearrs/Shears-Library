using System;
using UnityEngine;

namespace Shears.UI
{
    public class DraggableElement : UIManipulator
    {
        [Header("Dragger")]
        [SerializeField, RuntimeReadOnly]
        private bool isDraggable = true;

        [SerializeField, RuntimeReadOnly, Min(0.0f)]
        private float dragBeginTime = 0.05f;

        [SerializeField]
        private SpriteRenderer[] renderers;

        [SerializeField]
        private int dragSortOrder = 100;

        private int[] originalSortOrders;
        private Vector3 offset;
        private DragReceiver detectedReceiver;

        public DragReceiver DetectedReceiver => detectedReceiver;

        public event Action DragBegan;
        public event Action DragEnded;
        public event Action<DragReceiver> DragReceiverDetected;

        protected override void Awake()
        {
            base.Awake();

            if (renderers != null)
                originalSortOrders = new int[renderers.Length];
        }

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

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    originalSortOrders[i] = renderers[i].sortingOrder;
                    renderers[i].sortingOrder = dragSortOrder + i;
                }
            }

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

            if (UIElementEventSystem.TryRaycastElement(out DragReceiver receiver))
            {
                detectedReceiver = receiver;
                receiver.ReceiveDrag(this);
                DragReceiverDetected?.Invoke(receiver);
            }

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].sortingOrder = originalSortOrders[i];
            }

            DragEnded?.Invoke();
        }
    }
}
