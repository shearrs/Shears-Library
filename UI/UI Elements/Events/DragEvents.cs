using UnityEngine;

namespace Shears.UI
{
    public readonly struct DragBeginEvent : IUIEvent 
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 worldPointerOffset;

        public readonly Camera Camera => camera;
        public readonly Vector2 PointerPosition => pointerPosition;
        public readonly Vector3 PointerWorldOffset => worldPointerOffset;

        public DragBeginEvent(Camera camera, Vector2 pointerPosition, Vector3 worldPointerOffset)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.worldPointerOffset = worldPointerOffset;
        }
    }

    public readonly struct DragEvent : IUIEvent 
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 pointerWorldPosition;

        public readonly Camera Camera => camera;
        public readonly Vector2 PointerPosition => pointerPosition;
        public readonly Vector3 PointerWorldPosition => pointerWorldPosition;

        public DragEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.pointerWorldPosition = pointerWorldPosition;
        }
    }
    
    public readonly struct DragEndEvent : IUIEvent 
    {
        private readonly Camera camera;
        private readonly Vector2 pointerPosition;
        private readonly Vector3 pointerWorldPosition;

        public readonly Camera Camera => camera;
        public readonly Vector2 PointerPosition => pointerPosition;
        public readonly Vector3 PointerWorldPosition => pointerWorldPosition;

        public DragEndEvent(Camera camera, Vector2 pointerPosition, Vector3 pointerWorldPosition)
        {
            this.camera = camera;
            this.pointerPosition = pointerPosition;
            this.pointerWorldPosition = pointerWorldPosition;
        }
    }
}
