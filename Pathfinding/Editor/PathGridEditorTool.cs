using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    [EditorTool("Path Grid Tool", typeof(PathGrid))]
    public class PathGridEditorTool : EditorTool, IDrawSelectedHandles
    {
        private bool isActivated = false;
        private PathGrid grid;
        private int zDepth = 0;

        private PathGrid Grid
        {
            get
            {
                if (grid == null)
                    grid = target as PathGrid;

                return grid;
            }
        }

        public override void OnActivated()
        {
            isActivated = true;
        }

        public override void OnWillBeDeactivated()
        {
            isActivated = false;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView sceneView)
                return;

            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, sceneView.rootVisualElement.layout.height - 55, 300, 100));
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    sceneView.sceneViewState.alwaysRefresh = isActivated;

                    if (isActivated)
                    {
                        if (!sceneView.sceneViewState.fxEnabled)
                            sceneView.sceneViewState.fxEnabled = true;

                        zDepth = Mathf.Clamp(zDepth, 0, Grid.GridSize.z - 1);
                        zDepth = EditorGUILayout.IntSlider("Z Depth", zDepth, 0, Grid.GridSize.z - 1);
                    }
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        public void OnDrawHandles()
        {
            if (!isActivated)
            {
                var color = Color.white;
                color.a = 0.1f;
                Handles.color = color;

                foreach (var node in Grid.Nodes)
                    Handles.DrawWireCube(Grid.transform.TransformPoint(node.LocalPosition), Grid.NodeSize * Vector3.one);

                return;
            }

            Handles.color = Color.white;

            foreach (var node in Grid.Nodes)
            {
                if (node.GridPosition.z == zDepth)
                    CreateNodeHandle(node);
            }
        }

        private void CreateNodeHandle(PathNode node)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Vector3 handlePosition = Grid.transform.TransformPoint(node.LocalPosition);
            Vector3 handleSize = Grid.NodeSize * Vector3.one;
            Vector3 screenPosition = Handles.matrix.MultiplyPoint(handlePosition);

            Color color = Color.white;
            
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCube(screenPosition, Quaternion.identity, grid.NodeSize));

                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0 && HandleUtility.nearestControl == controlID)
                    {
                        GUIUtility.hotControl = controlID;
                        
                        Event.current.Use();
                    }

                    break;
                case EventType.Repaint:

                    if (HandleUtility.nearestControl != controlID)
                        break;

                    color = Color.yellow;
                    handleSize *= 0.9f;

                    break;
            }

            Handles.color = color;
            Handles.DrawWireCube(handlePosition, handleSize);
        }
    }
}
