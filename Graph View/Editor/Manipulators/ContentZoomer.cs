using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class ContentZoomer : Manipulator
    {
        private const float MIN_ZOOM = 0.25f;
        private const float MAX_ZOOM = 1f;
        private const float ZOOM_STEP = 0.02f;

        private readonly GraphView graphView;

        public ContentZoomer(GraphView graphView)
        {
            this.graphView = graphView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(OnWheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
        }

        private void OnWheel(WheelEvent evt)
        {
            var panel = target.panel;

            if (panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return;

            UpdateZoom(evt.localMousePosition, evt.delta.y);
            evt.StopPropagation();
        }

        private void UpdateZoom(Vector2 localMousePos, float wheelDelta)
        {
            Vector3 position = graphView.ViewTransform.position;
            Vector3 scale = graphView.ViewTransform.scale;

            float currentZoom = scale.y;
            float newZoom = CalculateNewZoom(currentZoom, wheelDelta);

            Vector2 previousMousePos = target.ChangeCoordinatesTo(graphView.ContentViewContainer, localMousePos);

            scale = new Vector3(newZoom, newZoom, 1);
            graphView.UpdateViewTransform(position, scale);

            Vector2 newMousePos = target.ChangeCoordinatesTo(graphView.ContentViewContainer, localMousePos);
            Vector2 movement = newZoom * (newMousePos - previousMousePos);

            position += (Vector3)movement;

            graphView.UpdateViewTransform(position, scale);
            graphView.SaveViewTransform();
        }

        private float CalculateNewZoom(float currentZoom, float wheelDelta)
        {
            float newZoom = currentZoom - (wheelDelta * ZOOM_STEP);

            return Mathf.Clamp(newZoom, MIN_ZOOM, MAX_ZOOM);
        }
    }
}
