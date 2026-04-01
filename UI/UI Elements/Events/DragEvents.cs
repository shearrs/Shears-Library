using UnityEngine;

namespace Shears.UI
{
    public class DragBeginEvent : UIEvent
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 worldPointerOffset;

        public Camera Camera => camera;
        public Vector2 PointerPosition => pointerPosition;
        public Vector3 PointerWorldOffset => worldPointerOffset;

        public DragBeginEvent(Camera camera, Vector2 pointerPosition, Vector3 worldPointerOffset)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.worldPointerOffset = worldPointerOffset;

            TrickleDown = false;
            BubbleUp = true;
        }
    }

    public class DragEvent : UIEvent
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 pointerWorldPosition;

        public Camera Camera => camera;
        public Vector2 PointerPosition => pointerPosition;
        public Vector3 PointerWorldPosition => pointerWorldPosition;

        public DragEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.pointerWorldPosition = pointerWorldPosition;

            TrickleDown = false;
            BubbleUp = true;
        }
    }

    public class DragEndEvent : UIEvent
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 pointerWorldPosition;

        public Camera Camera => camera;
        public Vector2 PointerPosition => pointerPosition;
        public Vector3 PointerWorldPosition => pointerWorldPosition;

        public DragEndEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.pointerWorldPosition = pointerWorldPosition;

            TrickleDown = false;
            BubbleUp = true;
        }
    }
}
