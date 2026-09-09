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
            if (node == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(
                grid.gameObject,
                $"Paint Node at {node.GridPosition}"
            );

            node.SetDefinition(State.SelectedDefinition);
            node.CreateNodeObject(grid);

            Undo.RegisterCompleteObjectUndo(grid, $"Paint Node at {node.GridPosition}");
            EditorUtility.SetDirty(grid);
        }

        private void PaintAutomaticNode(Vector3Int gridPosition)
        {
            var node = new GridNode(State.SelectedDefinition);

            Undo.RegisterFullObjectHierarchyUndo(
                grid.gameObject,
                $"Paint Automatic Node at {gridPosition}"
            );

            grid.SetNode(gridPosition, node);
            node.CreateNodeObject(grid);

            Undo.RegisterCompleteObjectUndo(grid, $"Paint Automatic Node at {gridPosition}");
            EditorUtility.SetDirty(grid);
        }

        private void EraseNode(GridNode node)
        {
            if (node == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(grid, $"Erase Node at {node.GridPosition}");
            node.SetDefinition(null);
            EditorUtility.SetDirty(grid);
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
