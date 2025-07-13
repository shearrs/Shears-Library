using System;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
        private bool isPlacing;

        public Action<IEdgeAnchorable, IEdgeAnchorable> TryPlaceEdgeCallback { get; set; }

        public event Action PlacingBegan;
        public event Action PlacingEnded;

        public EdgePlacer(GraphView graphView)
        {
            this.graphView = graphView;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public void BeginPlacing(IEdgeAnchorable anchor)
        {
            if (isPlacing)
                return;

            anchor1 = anchor;
            CreatePlacingEdge();

            graphView.ContentViewContainer.Add(placingEdge);
            placingEdge.SendToBack();
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);

            isPlacing = true;

            PlacingBegan?.Invoke();
        }

        public void EndPlacing()
        {
            if (!isPlacing)
                return;

            placingEdge.visible = false;
            graphView.ContentViewContainer.Remove(placingEdge);

            placingEdge = null;
            isPlacing = false;

            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);

            PlacingEnded?.Invoke();
        }

        private void CreatePlacingEdge()
        {
            placingEdge = new PlacingEdge(anchor1)
            {
                pickingMode = PickingMode.Ignore
            };

            placingEdge.SetAnchor2(graphView.ContentViewContainer.WorldToLocal(Mouse.current.position.ReadValue()));
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape && isPlacing)
                EndPlacing();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isPlacing)
                return;

            Vector2 mousePos = graphView.ContentViewContainer.WorldToLocal(evt.mousePosition);

            placingEdge.SetAnchor2(mousePos);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!isPlacing || !CanStopManipulation(evt))
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
