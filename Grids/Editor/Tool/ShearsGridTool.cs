using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Shears.Grids.Editor
{
    [EditorTool("Shears Grid Tool", typeof(ShearsGrid))]
    public class ShearsGridTool : EditorTool, IDrawSelectedHandles
    {
        private ShearsGrid grid;
        private ShearsGridToolGraphics graphics;
        private ShearsGridEditorWindow window;

        private ShearsGridToolState State => ShearsGridToolState.instance;

        private void OnEnable()
        {
            grid = target as ShearsGrid;
            graphics = new();

            var toolSO = new SerializedObject(this);
            State.Initialize(grid);

            State.SelectedNodeChanged += OnNodeSelected;
            State.AutomaticNodeChanged += OnAutomaticNodeChanged;
        }

        private void OnDisable()
        {
            State.SelectedNodeChanged -= OnNodeSelected;
            State.AutomaticNodeChanged -= OnAutomaticNodeChanged;
        }

        public override void OnActivated()
        {
            var sceneView = EditorWindow.GetWindow<SceneView>();
            sceneView.sceneViewState.fxEnabled = true;

            State.Active = true;

            window = ShearsGridEditorWindow.Open();
            window.InitializeState();
        }

        public override void OnWillBeDeactivated()
        {
            State.Active = false;
            window.ClearState();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.shift)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Alpha1:
                        State.SetTool(GridTool.Select);
                        break;
                    case KeyCode.Alpha2:
                        State.SetTool(GridTool.Paint);
                        break;
                    case KeyCode.Alpha3:
                        State.SetTool(GridTool.Eraser);
                        break;
                    case KeyCode.Alpha4:
                        State.SetTool(GridTool.Fill);
                        break;
                }
            }

            graphics?.DrawHandles();
            window.Repaint();
        }

        public void OnDrawHandles()
        {
            if (!State.Active)
                graphics?.DrawHandles();
        }

        private void OnNodeSelected(GridNode node)
        {
            switch (State.CurrentTool.Value)
            {
                case GridTool.Select:
                    SelectNode(node);
                    break;
                case GridTool.Paint:
                    PaintNode(node);
                    break;
                case GridTool.Eraser:
                    EraseNode(node);
                    break;
                case GridTool.Fill:
                    Fill(node);
                    break;
            }
        }

        private void OnAutomaticNodeChanged(Vector3Int gridPosition)
        {
            PaintAutomaticNode(gridPosition);
        }

        private void SelectNode(GridNode node)
        {
            window.InspectNode(node);
        }

        private void PaintNode(GridNode node)
        {
            if (State.BrushSize == Vector3Int.one)
                PaintNodeSingle(node);
            else
            {
                var min = node.GridPosition;
                var max = node.GridPosition + State.BrushSize;
                var size = max - min;
                CollectionUtil.GetPooled(out List<GridNode> nodes);

                grid.GetNodesInBounds(
                    new BoundsInt(min.x, min.y, min.z, size.x, size.y, size.z),
                    nodes
                );

                Undo.RegisterFullObjectHierarchyUndo(
                    grid.gameObject,
                    $"Paint Nodes at {min}-{max}"
                );

                foreach (var boundsNode in nodes)
                    PaintNodeImplementation(boundsNode);

                Undo.RegisterCompleteObjectUndo(grid, $"Paint Nodes at {min}-{max}");
                EditorUtility.SetDirty(grid);

                CollectionUtil.ReleasePooled(nodes);
            }
        }

        private void PaintNodeSingle(GridNode node)
        {
            if (node == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(
                grid.gameObject,
                $"Paint Node at {node.GridPosition}"
            );

            PaintNodeImplementation(node);

            Undo.RegisterCompleteObjectUndo(grid, $"Paint Node at {node.GridPosition}");
            EditorUtility.SetDirty(grid);
        }

        private void PaintNodeImplementation(GridNode node)
        {
            if (node == null)
                return;

            node.SetDefinition(State.SelectedDefinition);
            node.CreateNodeObject(grid);
        }

        private void PaintAutomaticNode(Vector3Int gridPosition)
        {
            if (State.BrushSize == Vector3Int.one)
                PaintAutomaticNodeSingle(gridPosition);
            else
            {
                var min = gridPosition;
                var max = gridPosition + State.BrushSize;

                Undo.RegisterFullObjectHierarchyUndo(
                    grid.gameObject,
                    $"Paint Automatic Nodes at {min}-{max}"
                );

                var offset = grid.ExpandToFit(min, max);

                for (int z = min.z; z < max.z; z++)
                {
                    for (int y = min.y; y < max.y; y++)
                    {
                        for (int x = min.x; x < max.x; x++)
                        {
                            var position = new Vector3Int(x, y, z) - offset;

                            PaintAutomaticNodeImplementation(position);
                        }
                    }
                }

                Undo.RegisterCompleteObjectUndo(grid, $"Paint Automatic Nodes at {min}-{max}");
                EditorUtility.SetDirty(grid);
            }
        }

        private void PaintAutomaticNodeSingle(Vector3Int gridPosition)
        {
            Undo.RegisterFullObjectHierarchyUndo(
                grid.gameObject,
                $"Paint Automatic Node at {gridPosition}"
            );

            PaintAutomaticNodeImplementation(gridPosition);

            Undo.RegisterCompleteObjectUndo(grid, $"Paint Automatic Node at {gridPosition}");
            EditorUtility.SetDirty(grid);
        }

        private void PaintAutomaticNodeImplementation(Vector3Int gridPosition)
        {
            var node = new GridNode(State.SelectedDefinition);
            grid.SetNode(gridPosition, node);
            node.CreateNodeObject(grid);
        }

        private void EraseNode(GridNode node)
        {
            if (State.BrushSize == Vector3Int.one)
                EraseNodeSingle(node);
            else
            {
                var min = node.GridPosition;
                var max = node.GridPosition + State.BrushSize;
                var size = max - min;
                CollectionUtil.GetPooled(out List<GridNode> nodes);

                grid.GetNodesInBounds(
                    new BoundsInt(min.x, min.y, min.z, size.x, size.y, size.z),
                    nodes
                );

                Undo.RegisterFullObjectHierarchyUndo(
                    grid.gameObject,
                    $"Erase Nodes at {min}-{max}"
                );

                foreach (var boundsNode in nodes)
                    EraseNodeImplementation(boundsNode);

                EditorUtility.SetDirty(grid);
                CollectionUtil.ReleasePooled(nodes);
            }
        }

        private void EraseNodeSingle(GridNode node)
        {
            if (node == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(grid, $"Erase Node at {node.GridPosition}");

            EraseNodeImplementation(node);

            EditorUtility.SetDirty(grid);
        }

        private void EraseNodeImplementation(GridNode node)
        {
            if (node == null)
                return;

            node.SetDefinition(null);
        }

        private void Fill(GridNode node)
        {
            if (node == null)
                return;

            var fillDefinition = node.Definition;
            var newDefinition = State.SelectedFillDefinition;

            Undo.RegisterFullObjectHierarchyUndo(grid, "Fill Grid");

            FillRecursive(node, fillDefinition, newDefinition);

            Undo.RegisterCompleteObjectUndo(grid, "Fill Grid");
            EditorUtility.SetDirty(grid);
        }

        private void FillRecursive(
            GridNode node,
            GridNodeDefinition fillDefinition,
            GridNodeDefinition newDefinition
        )
        {
            if (node.Definition == newDefinition || node.Definition != fillDefinition)
                return;

            Undo.RegisterCompleteObjectUndo(grid, "Fill Grid Node");
            node.SetDefinition(newDefinition);
            node.CreateNodeObject(grid);
            var gridPos = node.GridPosition;

            if (grid.TryGetNode(gridPos.With(y: gridPos.y + 1), out var upNode))
                FillRecursive(upNode, fillDefinition, newDefinition);

            if (grid.TryGetNode(gridPos.With(y: gridPos.y - 1), out var downNode))
                FillRecursive(downNode, fillDefinition, newDefinition);

            if (grid.TryGetNode(gridPos.With(x: gridPos.x + 1), out var rightNode))
                FillRecursive(rightNode, fillDefinition, newDefinition);

            if (grid.TryGetNode(gridPos.With(x: gridPos.x - 1), out var leftNode))
                FillRecursive(leftNode, fillDefinition, newDefinition);

            if (grid.TryGetNode(gridPos.With(z: gridPos.z + 1), out var forwardNode))
                FillRecursive(forwardNode, fillDefinition, newDefinition);

            if (grid.TryGetNode(gridPos.With(z: gridPos.z - 1), out var backwardNode))
                FillRecursive(backwardNode, fillDefinition, newDefinition);
        }
    }
}
