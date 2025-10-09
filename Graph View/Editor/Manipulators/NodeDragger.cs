using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace Shears.GraphViews.Editor
{
    public class NodeDragger : MouseManipulator
    {
        private const float MIN_DISTANCE = 5f;
        private const int PIXEL_STEP_SIZE = 20;

        private Vector2 mouseStart;
        private bool mouseDown = false;
        private bool isDragging = false;
        private GraphData graphData;
        private readonly Dictionary<GraphElement, Vector2> elementStartPositions = new();
        private readonly GraphView graphView;

        public NodeDragger(GraphView graphView)
        {
            this.graphView = graphView;
            isDragging = false;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(BeginDragging);
            target.RegisterCallback<MouseMoveEvent>(UpdateDragging);
            target.RegisterCallback<MouseUpEvent>(EndDragging);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(BeginDragging);
            target.UnregisterCallback<MouseMoveEvent>(UpdateDragging);
            target.UnregisterCallback<MouseUpEvent>(EndDragging);
        }

        public void SetGraphData(GraphData graphData)
        {
            this.graphData = graphData;
        }

        private void BeginDragging(MouseDownEvent evt)
        {
            if (isDragging)
            {
                evt.StopImmediatePropagation();
                return;
            }

            if (graphData.GetSelection().Count == 0)
                return;

            if (CanStartManipulation(evt))
            {
                RecordStartPositions();
                mouseDown = true;
                mouseStart = evt.localMousePosition;
                target.CaptureMouse();
            }
        }

        private void UpdateDragging(MouseMoveEvent evt)
        {
            if (!mouseDown)
                return;

            if (!isDragging)
            {
                if (Vector2.Distance(mouseStart, evt.localMousePosition) >= MIN_DISTANCE)
                    isDragging = true;
                else
                    return;
            }

            VisualElement eventTarget = evt.target as VisualElement;

            foreach (var elementData in graphData.GetSelection())
            {
                if (elementData is not GraphNodeData nodeData)
                    continue;

                var node = graphView.GetNode(nodeData);

                if (!elementStartPositions.TryGetValue(node, out Vector2 start))
                    continue;

                Vector2 nodeMouseStart = eventTarget.ChangeCoordinatesTo(node, mouseStart);
                Vector2 nodeMousePos = eventTarget.ChangeCoordinatesTo(node, evt.localMousePosition);
                
                Vector2 delta = nodeMousePos - nodeMouseStart;
                delta = RoundToPixelGrid(delta);

                Vector2 pos = start + delta;
                pos = RoundToPixelGrid(pos);

                node.style.translate = pos;
            }

            evt.StopPropagation();
        }

        private void EndDragging(MouseUpEvent evt)
        {
            mouseDown = false;
            target.ReleaseMouse();

            if (!isDragging || !CanStopManipulation(evt))
                return;

            isDragging = false;

            // if hovering over a multinode, put these nodes inside it
            // each with a little offset

            SaveEndPositions();
            evt.StopPropagation();
        }

        private void RecordStartPositions()
        {
            elementStartPositions.Clear();

            foreach (var elementData in graphData.GetSelection())
            {
                if (elementData is not GraphNodeData nodeData)
                    continue;

                var node = graphView.GetNode(nodeData);
                elementStartPositions.Add(node, nodeData.Position);
            }
        }

        private void SaveEndPositions()
        {
            graphView.Record("Drag Node");

            foreach (var elementData in graphData.GetSelection())
            {
                if (elementData is not GraphNodeData nodeData)
                    continue;
                
                var node = graphView.GetNode(nodeData);
                graphData.SetNodePosition(nodeData, node.resolvedStyle.translate);
            }

            graphView.Save();
        }

        private Vector2 RoundToPixelGrid(Vector2 pos)
        {
            Vector2Int posSteps = new(Mathf.RoundToInt(pos.x / PIXEL_STEP_SIZE), Mathf.RoundToInt(pos.y / PIXEL_STEP_SIZE));

            return posSteps * PIXEL_STEP_SIZE;
        }
    }
}
