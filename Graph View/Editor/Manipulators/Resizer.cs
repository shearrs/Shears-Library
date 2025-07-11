using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class Resizer : MouseManipulator
    {
        public enum ResizeDirection { Both, Width, Height }

        private readonly VisualElement resizeTarget;
        private readonly ResizeDirection resizeDirection;

        private bool active = false;
        private Vector2 start;
        private Vector2 startSize;

        public Resizer(VisualElement resizeTarget = null, ResizeDirection direction = ResizeDirection.Both)
        {
            resizeTarget ??= target;

            this.resizeTarget = resizeTarget;
            resizeDirection = direction;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(BeginResizing);
            target.RegisterCallback<MouseMoveEvent>(UpdateResizing);
            target.RegisterCallback<MouseUpEvent>(EndResizing);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(BeginResizing);
            target.UnregisterCallback<MouseMoveEvent>(UpdateResizing);
            target.UnregisterCallback<MouseUpEvent>(EndResizing);
        }

        private void BeginResizing(MouseDownEvent evt)
        {
            if (active)
            {
                evt.StopImmediatePropagation();
                return;
            }
            if (CanStartManipulation(evt))
            {
                start = evt.mousePosition;
                startSize = resizeTarget.layout.size;
                active = true;

                target.CaptureMouse();

                evt.StopPropagation();
            }
        }

        private void UpdateResizing(MouseMoveEvent evt)
        {
            if (!active || !target.HasMouseCapture())
                return;

            Vector2 delta = evt.mousePosition - start;

            if (resizeDirection == ResizeDirection.Both)
            {
                resizeTarget.style.height = startSize.y + delta.y;
                resizeTarget.style.width = startSize.x + delta.x;
            }
            else if (resizeDirection == ResizeDirection.Width)
                resizeTarget.style.width = startSize.x + delta.x;
            else
                resizeTarget.style.width = startSize.y + delta.y;

            evt.StopPropagation();
        }

        private void EndResizing(MouseUpEvent evt)
        {
            if (!active || !target.HasMouseCapture() || !CanStopManipulation(evt))
                return;

            active = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}
