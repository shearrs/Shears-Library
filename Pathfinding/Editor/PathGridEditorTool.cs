using Shears.Editor;
using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    [EditorTool("Path Grid Tool", typeof(PathGrid))]
    public class PathGridEditorTool : EditorTool, IDrawSelectedHandles
    {
        #region Variables
        [SerializeField] private PGETSettings settings;
        private PGETUI ui;
        private PGETPainter painter;

        private VisualElement root;
        private SceneView sceneView;
        private PathGrid grid;
        private SerializedObject editorSO;
        private SerializedObject gridSO;
        #endregion

        private void OnEnable()
        {
            grid = target as PathGrid;
            editorSO = new SerializedObject(this);
            gridSO = new SerializedObject(grid);

            settings = new(editorSO, grid, gridSO);
            Debug.Log("create settings");
            painter = new(settings);
        }

        public override void OnActivated()
        {
            settings.SetActivated(true);

            CreateGUI();

            ui.TypeSelected += OnTypeSelected;
            ui.PaintRequested += OnPaintRequested;
        }

        public override void OnWillBeDeactivated()
        {
            settings.SetActivated(false);

            sceneView.rootVisualElement.Remove(root);

            ui.TypeSelected -= OnTypeSelected;
            ui.PaintRequested -= OnPaintRequested;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.shift)
            {
                if (Event.current.keyCode == KeyCode.Alpha1)
                {
                    settings.ZDepthProp.intValue = Mathf.Max(0, settings.ZDepth - 1);
                    editorSO.ApplyModifiedProperties();
                }
                else if (Event.current.keyCode == KeyCode.Alpha2)
                {
                    settings.ZDepthProp.intValue = Mathf.Min(grid.GridSize.z - 1, settings.ZDepth + 1);
                    editorSO.ApplyModifiedProperties();
                }
            }
        }

        private void CreateGUI()
        {
            sceneView = EditorWindow.GetWindow<SceneView>();

            if (!sceneView.sceneViewState.fxEnabled)
                sceneView.sceneViewState.fxEnabled = true;

            root = new();
            root.style.marginTop = StyleKeyword.Auto;
            root.style.marginRight = StyleKeyword.Auto;
            root.style.marginLeft = 10;
            root.style.marginBottom = 10;

            ui = new(settings);

            root.Add(ui);
            sceneView.rootVisualElement.Add(root);
        }

        void IDrawSelectedHandles.OnDrawHandles()
        {
            ui?.DrawHandles();
        }

        private void OnTypeSelected(Type type)
        {
            if (type != null)
                settings.NodeDataProp.boxedValue = (PathNodeData)Activator.CreateInstance(type);
            else
                settings.NodeDataProp.boxedValue = null;

            settings.EditorSO.ApplyModifiedProperties();
        }

        private void OnPaintRequested(PathNode node, SerializedProperty nodeProp)
        {
            painter.PaintNode(node, nodeProp);
            settings.GridSO.ApplyModifiedProperties();
        }

        public static Vector3 GetWorldPosition(PathGrid grid, PathNode node)
        {
            return grid.transform.TransformPoint(grid.NodeSize * (Vector3)node.GridPosition);
        }
    }
}
