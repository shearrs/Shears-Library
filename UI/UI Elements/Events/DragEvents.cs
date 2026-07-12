using UnityEngine;

namespace Shears.UI
{
    public class DragBeginEvent : UIEvent
    {
        public Camera Camera { get; }
        public Vector2 PointerPosition { get; }
        public Vector3 PointerWorldOffset { get; }

        public DragBeginEvent(Camera camera, Vector2 pointerPosition, Vector3 worldPointerOffset)
        {
            Camera = camera;
            PointerPosition = pointerPosition;
            PointerWorldOffset = worldPointerOffset;

            TrickleDown = false;
            BubbleUp = true;
        }
    }

    public class DragEvent : UIEvent
    {
        public Camera Camera { get; }
        public Vector2 PointerPosition { get; }
        public Vector3 PointerWorldPosition { get; }

        public DragEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            Camera = camera;
            PointerPosition = pointerPosition;
            PointerWorldPosition = pointerWorldPosition;

            BubbleUp = true;
        }
    }

    public class DragEndEvent : UIEvent
    {
        public Camera Camera { get; }
        public Vector2 PointerPosition { get; }
        public Vector3 PointerWorldPosition { get; }

        public DragEndEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            Camera = camera;
            PointerPosition = pointerPosition;
            PointerWorldPosition = pointerWorldPosition;

            BubbleUp = true;
        }
    }

    public class DragReleaseTargetEvent : UIEvent
    {
        public Vector2 PointerPosition { get; }
        public Vector3 PointerWorldPosition { get; }
        public UIElement DraggedElement { get; }

        public DragReleaseTargetEvent(
            Vector2 pointerPosition,
            Vector3 pointerWorldPosition,
            UIElement draggedElement
        )
        {
            PointerPosition = pointerPosition;
            PointerWorldPosition = pointerWorldPosition;
            DraggedElement = draggedElement;
        }
    }

    public class DragReleaseEvent : UIEvent
    {
        public Vector2 PointerPosition { get; }
        public Vector3 PointerWorldPosition { get; }
        public UIElement ReleaseTarget { get; }

        public DragReleaseEvent(
            Vector2 pointerPosition,
            Vector3 pointerWorldPosition,
            UIElement target
        )
        {
            PointerPosition = pointerPosition;
            PointerWorldPosition = pointerWorldPosition;
            ReleaseTarget = target;
        }
    }
}
