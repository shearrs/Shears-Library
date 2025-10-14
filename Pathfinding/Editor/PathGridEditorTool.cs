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
        private bool isActivated = false;
        private GenericMenu typeMenu;
        private VisualElement root;
        private SceneView sceneView;
        private PathGrid grid;
        private int zDepth = 0;
        private Type nodeDataType;

        private PathGrid Grid
        {
            get
            {
                if (grid == null)
                    grid = target as PathGrid;

                return grid;
            }
        }

        private void OnEnable()
        {
            CreateTypeMenu();
        }

        public override void OnActivated()
        {
            isActivated = true;

            sceneView = EditorWindow.GetWindow<SceneView>();

            if (!sceneView.sceneViewState.fxEnabled)
                sceneView.sceneViewState.fxEnabled = true;

            CreateGUI();
        }

        public override void OnWillBeDeactivated()
        {
            isActivated = false;
            sceneView.rootVisualElement.Remove(root);
        }

        private void CreateGUI()
        {
            root = new();
            
            root.style.marginTop = StyleKeyword.Auto;
            root.style.marginRight = StyleKeyword.Auto;
            root.style.marginLeft = 10;
            root.style.marginBottom = 10;
            root.SetAllPadding(4);
            root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            root.SetAllBorderColors(new Color(0.1f, 0.1f, 0.1f, 0.8f));
            root.SetAllBorders(1);

            var depthSlider = new SliderInt("Z Depth", 0, grid.GridSize.z)
            {
                value = zDepth,
                showInputField = true
            };
            depthSlider.style.marginBottom = 8;
            depthSlider.labelElement.style.minWidth = 80;
            depthSlider.hierarchy[1].hierarchy[0].style.minWidth = 60;

            depthSlider.RegisterValueChangedCallback(OnDepthSliderChanged);

            var typeContainer = new VisualElement();
            typeContainer.style.flexDirection = FlexDirection.Row;

            var typeLabel = new Label("Data Type");

            string typeName = nodeDataType == null ? "None" : nodeDataType.Name;
            var typeButton = new Button(typeMenu.ShowAsContext)
            {
                text = typeName
            };
            typeButton.style.marginLeft = 4;
            typeButton.style.flexGrow = 1;

            typeContainer.AddAll(typeLabel, typeButton);

            root.Add(depthSlider);
            root.Add(typeContainer);
            sceneView.rootVisualElement.Add(root);
        }

        private void OnDepthSliderChanged(ChangeEvent<int> evt)
        {
            zDepth = evt.newValue;
        }

        private void CreateTypeMenu()
        {
            typeMenu = new GenericMenu();

            typeMenu.AddItem(new("None"), false, () => OnTypeSelected(null));

            foreach (var type in TypeCache.GetTypesDerivedFrom<PathNodeData>())
                typeMenu.AddItem(new(type.Name), false, () => OnTypeSelected(type));
        }

        private void OnTypeSelected(Type type)
        {
            nodeDataType = type;
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
                case EventType.MouseDown:
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
