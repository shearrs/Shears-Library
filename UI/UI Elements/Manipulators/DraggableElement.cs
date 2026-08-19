using System;
using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.UI
{
    public class DraggableElement : UIManipulator
    {
        private readonly List<SpriteRenderer> renderers = new();
        private readonly int dragSortOrder = 100;
        private readonly int[] originalSortOrders;
        private readonly Ref<bool> isDragged = new();
        private Vector3 offset;

        public IReadOnlyRef<bool> IsDragged => isDragged;

        public event Action DragBegan;
        public event Action DragEnded;
        public event Action<UIElement> DragReleased;

        public DraggableElement(UIElement element)
            : base(element)
        {
            element.GetComponentsInChildren(renderers);
            originalSortOrders = new int[renderers.Count];
        }

        protected override void RegisterEvents()
        {
            Element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            Element.RegisterEvent<DragEvent>(OnDrag);
            Element.RegisterEvent<DragEndEvent>(OnDragEnd);
            Element.RegisterEvent<DragReleaseEvent>(OnDragRelease);
        }

        protected override void DeregisterEvents()
        {
            Element.UnregisterEvent<DragBeginEvent>(OnDragBegin);
            Element.UnregisterEvent<DragEvent>(OnDrag);
            Element.UnregisterEvent<DragEndEvent>(OnDragEnd);
            Element.UnregisterEvent<DragReleaseEvent>(OnDragRelease);
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            evt.PreventDefault();

            UIElementEventSystem.SetDraggedElement(Element);
            offset = evt.PointerWorldOffset;

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    originalSortOrders[i] = renderers[i].sortingOrder;
                    renderers[i].sortingOrder = originalSortOrders[i] + dragSortOrder + i;
                }
            }

            isDragged.Value = true;

            DragBegan?.Invoke();
        }

        private void OnDrag(DragEvent evt)
        {
            evt.PreventDefault();

            const float MOVE_SPEED = 4.0f;

            Vector3 pointerWorld = evt.PointerWorldPosition;
            Vector3 targetPosition = pointerWorld + offset;
            Element.transform.position = Vector3.MoveTowards(
                Element.transform.position,
                targetPosition,
                MOVE_SPEED * Time.deltaTime
            );
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            evt.PreventDefault();

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Count; i++)
                    renderers[i].sortingOrder = originalSortOrders[i];
            }

            isDragged.Value = false;

            DragEnded?.Invoke();
        }

        private void OnDragRelease(DragReleaseEvent evt)
        {
            evt.PreventDefault();

            DragReleased?.Invoke(evt.ReleaseTarget);
        }
    }
}
