using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    [EditorTool("Path Grid Group Tool", typeof(IPathGrid))]
    public class PathGridGroupEditorTool : EditorTool, IDrawSelectedHandles
    {
        [SerializeField]
        private IPGETSettings settings;

        private IPathGrid grid;
        private IPGETUI ui;
        private IPGETPainter painter;
        private VisualElement root;
        private SceneView sceneView;

        private void OnEnable()
        {
            grid = target as IPathGrid;
            settings = new(this, grid);
            ui = new(settings);
            painter = new(settings);
        }

        public override void OnActivated()
        {
            if (ui == null)
                Debug.LogError("ui is null???");

            settings.IsActivated = true;

            CreateGUI();

            ui.TypeSelected += OnTypeSelected;
            ui.PaintRequested += OnPaintRequested;
        }

        public override void OnWillBeDeactivated()
        {
            settings.IsActivated = false;

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
                    settings.ApplyModifiedProperties();
                }
                else if (Event.current.keyCode == KeyCode.Alpha2)
                {
                    settings.ZDepthProp.intValue = Mathf.Min(
                        grid.GridSize.z - 1,
                        settings.ZDepth + 1
                    );
                    settings.ApplyModifiedProperties();
                }
            }
        }

        public void OnDrawHandles()
        {
            ui?.DrawHandles();
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

            root.Add(ui);
            sceneView.rootVisualElement.Add(root);
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
            var gridSO = settings.GridSO;

            gridSO.Update();
            painter.PaintNode(node, nodeProp);
            gridSO.ApplyModifiedProperties();
        }
    }
}
