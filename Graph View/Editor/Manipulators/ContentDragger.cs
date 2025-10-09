using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class ContentDragger : MouseManipulator
    {
        private readonly GraphView graphView;
        private bool isDragging = false;

        public ContentDragger(GraphView graphView)
        {
            this.graphView = graphView;

            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Alt
            });

            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.MiddleMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (isDragging)
                evt.StopImmediatePropagation();

            if (CanStartManipulation(evt))
            {
                isDragging = true;

                target.CaptureMouse();
                evt.StopImmediatePropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isDragging)
                return;

            Vector3 newPosition = graphView.ViewTransform.Position + new Vector3(evt.mouseDelta.x, evt.mouseDelta.y, 0);
            graphView.UpdateViewTransform(newPosition, graphView.ViewTransform.Scale);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!isDragging || !CanStopManipulation(evt))
                return;

            isDragging = false;

            target.ReleaseMouse();
            evt.StopImmediatePropagation();

            graphView.SaveViewTransform();
        }
    }
}
