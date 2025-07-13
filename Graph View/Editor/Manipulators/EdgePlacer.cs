using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class EdgePlacer : MouseManipulator
    {
        private readonly GraphView graphView;
        private PlacingEdge placingEdge;
        private IEdgeAnchorable anchor1;

        public bool IsPlacing { get; private set; }

        public Action<IEdgeAnchorable, IEdgeAnchorable> TryPlaceEdgeCallback { get; set; }

        public EdgePlacer(GraphView graphView)
        {
            this.graphView = graphView;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseMoveEvent>(UpdateMouseAnchor);
            target.RegisterCallback<MouseDownEvent>(TryCreateEdge);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseMoveEvent>(UpdateMouseAnchor);
            target.UnregisterCallback<MouseDownEvent>(TryCreateEdge);
        }

        public void BeginPlacing(IEdgeAnchorable anchor)
        {
            if (IsPlacing)
                return;

            anchor1 = anchor;

            placingEdge = new(anchor)
            {
                pickingMode = PickingMode.Ignore
            };
            placingEdge.SetAnchor2(graphView.ContentViewContainer.WorldToLocal(Mouse.current.position.ReadValue()));

            graphView.ContentViewContainer.Add(placingEdge);
            placingEdge.SendToBack();

            IsPlacing = true;
        }

        public void EndPlacing()
        {
            if (!IsPlacing)
                return;

            placingEdge.visible = false;
            graphView.ContentViewContainer.Remove(placingEdge);

            placingEdge = null;
            IsPlacing = false;
        }

        private void UpdateMouseAnchor(MouseMoveEvent evt)
        {
            if (!IsPlacing)
                return;

            Vector2 mousePos = graphView.ContentViewContainer.WorldToLocal(evt.mousePosition);

            placingEdge.SetAnchor2(mousePos);
        }

        private void TryCreateEdge(MouseDownEvent evt)
        {
            if (!IsPlacing)
                return;

            if (evt.target is IEdgeAnchorable anchor2)
            {
                TryPlaceEdgeCallback?.Invoke(anchor1, anchor2);
                TryPlaceEdgeCallback = null;
            }

            EndPlacing();
            evt.StopImmediatePropagation();
        }
    }
}
