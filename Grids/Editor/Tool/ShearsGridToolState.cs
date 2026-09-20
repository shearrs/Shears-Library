using System;
using Shears.Logging;
using UnityEditor;
using UnityEngine;

namespace Shears.Grids.Editor
{
    [FilePath("Shears Grid/State.txt", FilePathAttribute.Location.PreferencesFolder)]
    public class ShearsGridToolState : ScriptableSingleton<ShearsGridToolState>
    {
        private static readonly Vector3Int NO_SELECTION = -Vector3Int.one;

        [SerializeField]
        private Ref<GridTool> currentTool = new(GridTool.Select);

        [SerializeField]
        private Vector3Int selectedNodePosition = NO_SELECTION;

        [SerializeField]
        private ViewMode viewMode = ViewMode.Explicit;

        [SerializeField, ShowIf(nameof(viewMode), ViewMode.Explicit)]
        private Axis viewAxis = Axis.Z;

        [SerializeField, ShowIf(nameof(viewMode), ViewMode.Explicit)]
        private int viewDepth = 0;

        [SerializeField]
        private Vector3Int brushSize = Vector3Int.one;

        [SerializeField]
        private GridNodeDefinition selectedDefinition;

        [SerializeField]
        private GridNodeDefinition selectedFillDefinition;

        private SerializedProperty nodesProperty;

        public bool Active { get; set; }
        public ViewMode ViewingMode => viewMode;
        public Axis ViewAxis => viewAxis;
        public int ViewDepth => viewDepth;
        public Vector3Int BrushSize => brushSize;
        public ShearsGrid Grid { get; private set; }
        public IReadOnlyRef<GridTool> CurrentTool => currentTool;
        public GridNode SelectedNode => GetSelectedNode();
        public GridNodeDefinition SelectedDefinition => selectedDefinition;
        public GridNodeDefinition SelectedFillDefinition => selectedFillDefinition;
        public Vector3Int AutomaticNode { get; private set; }

        public SerializedObject GridSO { get; private set; }
        public SerializedProperty GridSizeProperty { get; private set; }
        public SerializedProperty ViewModeProperty { get; private set; }
        public SerializedProperty ViewAxisProperty { get; private set; }
        public SerializedProperty ViewDepthProperty { get; private set; }
        public SerializedProperty BrushSizeProperty { get; private set; }
        public SerializedProperty SelectedDefinitionProperty { get; private set; }
        public SerializedProperty SelectedFillDefinitionProperty { get; private set; }

        public event Action<GridNode> SelectedNodeChanged;
        public event Action<Vector3Int> AutomaticNodeChanged;

        public enum ViewMode
        {
            Explicit,
            Automatic,
        }

        public enum Axis
        {
            X,
            Y,
            Z,
        }

        public void Initialize(ShearsGrid grid)
        {
            Grid = grid;
            GridSO = new(grid);
            GridSizeProperty = GridSO.FindProperty("size");
            nodesProperty = GridSO.FindProperty("nodes");

            var stateSO = new SerializedObject(this);
            ViewModeProperty = stateSO.FindProperty(nameof(viewMode));
            ViewAxisProperty = stateSO.FindProperty(nameof(viewAxis));
            ViewDepthProperty = stateSO.FindProperty(nameof(viewDepth));
            BrushSizeProperty = stateSO.FindProperty(nameof(brushSize));
            SelectedDefinitionProperty = stateSO.FindProperty(nameof(selectedDefinition));
            SelectedFillDefinitionProperty = stateSO.FindProperty(nameof(selectedFillDefinition));

            ClampViewDepth();
        }

        public void SetTool(GridTool tool)
        {
            if (currentTool == tool)
                return;

            currentTool.Value = tool;
            Save(true);
        }

        public void SetSelectedNode(GridNode node)
        {
            if (node == null)
                selectedNodePosition = NO_SELECTION;
            else
                selectedNodePosition = node.GridPosition;

            SelectedNodeChanged?.Invoke(node);
            Save(true);
        }

        public void SetAutomaticNode(Vector3Int gridPosition)
        {
            AutomaticNode = gridPosition;
            AutomaticNodeChanged?.Invoke(gridPosition);
        }

        public SerializedProperty GetNodeProperty(Vector3Int gridPosition)
        {
            int index = Grid.GetNodeIndex(gridPosition);

            if (index > nodesProperty.arraySize)
            {
                SHLogger.LogError($"Index {index} is out of bounds of the grid.");
                return null;
            }

            return nodesProperty.GetArrayElementAtIndex(index);
        }

        private void ClampViewDepth()
        {
            int max = viewAxis switch
            {
                Axis.X => Grid.Size.x,
                Axis.Y => Grid.Size.y,
                Axis.Z => Grid.Size.z,
                _ => 0,
            };

            max--;

            ViewDepthProperty.intValue = Mathf.Min(ViewDepthProperty.intValue, max);
            ViewDepthProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private GridNode GetSelectedNode()
        {
            if (selectedNodePosition == NO_SELECTION || Grid == null)
                return null;

            if (!Grid.TryGetNode(selectedNodePosition, out var node))
                selectedNodePosition = NO_SELECTION;

            return node;
        }
    }
}
