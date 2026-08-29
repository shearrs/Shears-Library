using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Shears.Grids.Editor
{
    public class ShearsGridToolGraphics
    {
        private const double AUTOMATIC_DELAY = 0.5f;
        private static readonly Color PREVIEW_COLOR = Color.white.With(a: 0.35f);
        private static readonly Color HOVER_COLOR = Color.yellowNice;

        private readonly HashSet<int> hoveredHandles = new();
        private readonly Dictionary<int, GridNode> nodeHandles = new();
        private bool isMouseDown = false;
        private double previousAutomaticTime = 0.0f;

        private ShearsGridToolState State => ShearsGridToolState.instance;
        private ShearsGrid Grid => State.Grid;

        public void DrawHandles()
        {
            nodeHandles.Clear();

            if (State.Active)
            {
                for (int i = 0; i < Grid.Nodes.Count; i++)
                {
                    var node = Grid.Nodes[i];

                    if (ShouldDrawNodeHandle(node))
                        DrawNodeHandle(node);
                }
            }
            else
                DrawPreviewHandle();

            if (isMouseDown)
                HandleUtility.Repaint();
        }

        private bool ShouldDrawNodeHandle(GridNode node)
        {
            return State.ViewingMode switch
            {
                ShearsGridToolState.ViewMode.Explicit => ShouldDrawExplicit(node),
                ShearsGridToolState.ViewMode.Automatic => node.TryGetAutomaticEditorColor(out _),
                _ => false,
            };
        }

        private void DrawNodeHandle(GridNode node)
        {
            var matrix = Matrix4x4.TRS(
                Grid.transform.position,
                Grid.transform.rotation,
                Vector3.one
            );

            using (new Handles.DrawingScope(matrix))
            {
                Handles.zTest = CompareFunction.LessEqual;

                switch (State.ViewingMode)
                {
                    case ShearsGridToolState.ViewMode.Explicit:
                        DrawNodeHandleExplicit(node);
                        break;
                    case ShearsGridToolState.ViewMode.Automatic:
                        DrawNodeHandleAutomatic(node);
                        break;
                }

                Handles.zTest = CompareFunction.Always;
            }
        }

        private void DrawNodeHandleExplicit(GridNode node)
        {
            var localPosition = Grid.GetLocalPosition(node);
            float sizeFactor = Grid.NodeSize;
            var size = sizeFactor * Vector3.one;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var tool = State.CurrentTool.Value;
            nodeHandles[controlID] = node;

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    float distance = HandleUtility.DistanceToCube(
                        localPosition,
                        Quaternion.identity,
                        Grid.NodeSize
                    );

                    var worldGUIPosition = HandleUtility.WorldToGUIPointWithDepth(localPosition);
                    distance += 0.001f * worldGUIPosition.z;

                    HandleUtility.AddControl(controlID, distance);

                    break;
                case EventType.MouseDown:
                    isMouseDown = Event.current.button == 0;

                    if (isMouseDown && HandleUtility.nearestControl == controlID)
                    {
                        GUIUtility.hotControl = controlID;
                        SelectNode(controlID);
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    isMouseDown = false;
                    hoveredHandles.Clear();
                    break;
                case EventType.Repaint:
                    var color = node.EditorColor;
                    bool hovered = false;

                    if (HandleUtility.nearestControl == controlID)
                    {
                        if (isMouseDown)
                            SelectNode(controlID, tool == GridTool.Select);

                        hovered = true;
                    }

                    if (hovered)
                    {
                        Handles.color = HOVER_COLOR;
                        size *= 0.9f;
                        Handles.zTest = CompareFunction.Always;
                        Handles.DrawWireCube(localPosition, size);
                    }
                    else
                    {
                        Handles.color = color.With(a: 0.25f);
                        Handles.zTest = CompareFunction.Greater;
                        Handles.DrawWireCube(localPosition, size);

                        Handles.color = color;
                        Handles.zTest = CompareFunction.LessEqual;
                        Handles.DrawWireCube(localPosition, size);
                    }

                    break;
            }
        }

        private bool ShouldDrawExplicit(GridNode node)
        {
            var gridPosition = node.GridPosition;

            return State.ViewAxis switch
            {
                ShearsGridToolState.Axis.X => gridPosition.x == State.ViewDepth,
                ShearsGridToolState.Axis.Y => gridPosition.y == State.ViewDepth,
                ShearsGridToolState.Axis.Z => gridPosition.z == State.ViewDepth,
                _ => false,
            };
        }

        private void DrawNodeHandleAutomatic(GridNode node)
        {
            var localPosition = Grid.GetLocalPosition(node);
            float sizeFactor = Grid.NodeSize;
            var size = sizeFactor * Vector3.one;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var tool = State.CurrentTool.Value;
            nodeHandles[controlID] = node;

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    float distance = HandleUtility.DistanceToCube(
                        localPosition,
                        Quaternion.identity,
                        sizeFactor
                    );

                    var worldGUIPosition = HandleUtility.WorldToGUIPointWithDepth(localPosition);
                    distance += 0.001f * worldGUIPosition.z;

                    HandleUtility.AddControl(controlID, distance);

                    break;
                case EventType.MouseDown:
                    isMouseDown = Event.current.button == 0;

                    if (isMouseDown && HandleUtility.nearestControl == controlID)
                    {
                        GUIUtility.hotControl = controlID;

                        if (tool == GridTool.Paint)
                        {
                            if (AutomaticRaycast(node, size, out var targetPosition))
                            {
                                PaintAutomaticNode(targetPosition);
                                previousAutomaticTime = Time.timeAsDouble;
                            }
                        }
                        else
                        {
                            SelectNode(controlID);
                            previousAutomaticTime = Time.timeAsDouble;
                        }

                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    isMouseDown = false;
                    hoveredHandles.Clear();
                    break;
                case EventType.Repaint:
                    node.TryGetAutomaticEditorColor(out var color);

                    if (HandleUtility.nearestControl == controlID)
                    {
                        var timeOffset = Time.timeAsDouble - previousAutomaticTime;

                        if (tool == GridTool.Select)
                        {
                            if (isMouseDown)
                            {
                                SelectNode(controlID);
                                previousAutomaticTime = Time.timeAsDouble;
                            }

                            color = HOVER_COLOR;
                            size *= 0.9f;
                        }
                        else if (tool == GridTool.Paint)
                        {
                            if (AutomaticRaycast(node, size, out var targetPosition))
                            {
                                var offset = targetPosition - node.GridPosition;

                                Handles.color = PREVIEW_COLOR;
                                Handles.DrawWireCube(localPosition + offset, size);

                                if (isMouseDown && timeOffset >= AUTOMATIC_DELAY)
                                {
                                    PaintAutomaticNode(targetPosition);
                                    previousAutomaticTime = Time.timeAsDouble;
                                }
                            }
                        }
                        else if (tool == GridTool.Eraser)
                        {
                            if (isMouseDown && timeOffset >= AUTOMATIC_DELAY)
                            {
                                SelectNode(controlID, true);
                                previousAutomaticTime = Time.timeAsDouble;
                            }

                            color = HOVER_COLOR;
                            size *= 0.9f;
                            Handles.zTest = CompareFunction.Always;
                        }
                    }

                    Handles.color = color;
                    Handles.DrawWireCube(localPosition, size);

                    break;
            }
        }

        private bool AutomaticRaycast(GridNode node, Vector3 size, out Vector3Int targetPosition)
        {
            targetPosition = Vector3Int.zero;

            var localPosition = Grid.GetLocalPosition(node);
            var worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var localRayOrigin =
                Grid.transform.InverseTransformPoint(worldRay.origin) - localPosition;
            var localRayDirection = Grid.transform.InverseTransformDirection(worldRay.direction);
            var localBounds = new Bounds(Vector3.zero, size);

            if (localBounds.IntersectRay(new(localRayOrigin, localRayDirection), out float dist))
            {
                var localHitPoint = localRayOrigin + dist * localRayDirection;
                var localNormal = localHitPoint.GetMostAlignedAxis();
                targetPosition = node.GridPosition + localNormal.RoundToInt();

                return true;
            }

            return false;
        }

        private void DrawPreviewHandle()
        {
            var position = Grid.GetCenter();
            var size = Grid.Size;
            var color = PREVIEW_COLOR;

            Handles.color = color;
            Handles.DrawWireCube(position, size);
        }

        private void SelectNode(int controlID, bool ignoreHovered = false)
        {
            if (!ignoreHovered && hoveredHandles.Contains(controlID))
                return;

            if (!nodeHandles.TryGetValue(controlID, out var node))
                return;

            hoveredHandles.Add(controlID);
            State.SetSelectedNode(node);
        }

        private void PaintAutomaticNode(Vector3Int gridPosition)
        {
            State.SetAutomaticNode(gridPosition);
        }
    }
}
