using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class RectangleSelector : MouseManipulator
    {
        const float MIN_DISTANCE = 10f;

        private readonly VisualElement rectangle;
        private readonly List<ISelectable> selectedElements = new();
        private readonly GraphView graphView;

        private bool rectangleStyleSolved = true;
        private bool mouseDown = false;
        private bool active = false;
        private Vector2 start = Vector2.zero;

        public RectangleSelector(GraphView graphView)
        {
            this.graphView = graphView;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });

            rectangle = new()
            {
                pickingMode = PickingMode.Ignore,
                visible = false
            };

            rectangle.AddToClassList("rectangleSelector");
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(BeginSelection);
            target.RegisterCallback<MouseMoveEvent>(UpdateSelection);
            target.RegisterCallback<MouseUpEvent>(EndSelection);

            target.Add(rectangle);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(BeginSelection);
            target.UnregisterCallback<MouseMoveEvent>(UpdateSelection);
            target.UnregisterCallback<MouseUpEvent>(EndSelection);

            target.Remove(rectangle);
        }

        private void OnRectangleGeometryChanged(GeometryChangedEvent evt)
        {
            rectangleStyleSolved = true;
            rectangle.UnregisterCallback<GeometryChangedEvent>(OnRectangleGeometryChanged);
        }

        private void BeginSelection(MouseDownEvent evt)
        {
            if (graphView.SelectionCount > 0)
                return;

            if (mouseDown)
            {
                evt.StopImmediatePropagation();
                return;
            }

            if (!CanStartManipulation(evt))
                return;

            mouseDown = true;
            start = evt.localMousePosition;
            target.CaptureMouse();

            evt.StopImmediatePropagation();
        }

        private void UpdateSelection(MouseMoveEvent evt)
        {
            if (!mouseDown)
                return;

            if (!active)
            {
                if (Vector3.Distance(start, evt.localMousePosition) >= MIN_DISTANCE)
                {
                    rectangle.visible = true;
                    active = true;
                }
                else
                    return;
            }

            UpdateRectangleSize(evt.localMousePosition);
            StoreSelectedElements();
        }

        private void EndSelection(MouseUpEvent evt)
        {
            if (!mouseDown)
                return;

            mouseDown = false;
            target.ReleaseMouse();

            if (!active || !CanStopManipulation(evt))
                return;

            active = false;
            rectangle.visible = false;
            rectangleStyleSolved = false;
            rectangle.RegisterCallback<GeometryChangedEvent>(OnRectangleGeometryChanged);

            SelectElements();

            evt.StopImmediatePropagation();
        }

        private void UpdateRectangleSize(Vector2 mousePos)
        {
            if (mousePos.x > start.x)
            {
                rectangle.style.left = start.x;
                rectangle.style.right = target.layout.width - mousePos.x;
            }
            else
            {
                rectangle.style.left = mousePos.x;
                rectangle.style.right = target.layout.width - start.x;
            }

            if (mousePos.y > start.y)
            {
                rectangle.style.top = start.y;
                rectangle.style.bottom = target.layout.height - mousePos.y;
            }
            else
            {
                rectangle.style.top = mousePos.y;
                rectangle.style.bottom = target.layout.height - start.y;
            }
        }

        private void StoreSelectedElements()
        {
            foreach (var element in selectedElements)
                element.Deselect();

            selectedElements.Clear();

            if (!rectangleStyleSolved)
                return;

            foreach (var element in graphView.GetElements())
            {
                if (element is not ISelectable selectable)
                    continue;

                if (rectangle.worldBound.Overlaps(element.worldBound, true))
                    selectedElements.Add(selectable);
            }

            foreach (var element in selectedElements)
                element.Select();
        }

        private void SelectElements()
        {
            if (selectedElements.Count == 0)
                graphView.Select(null);
            else
                graphView.SelectAll(selectedElements);

            selectedElements.Clear();
        }
    }
}
