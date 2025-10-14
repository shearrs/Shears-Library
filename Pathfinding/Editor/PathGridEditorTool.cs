using Shears.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    [EditorTool("Path Grid Tool", typeof(PathGrid))]
    public class PathGridEditorTool : EditorTool, IDrawSelectedHandles
    {
        [SerializeReference] private PathNodeData nodeData;
        
        private readonly Dictionary<int, PathNode> nodeHandles = new();
        private readonly List<int> hoveredHandles = new();

        private bool isActivated = false;
        private bool isMouseDown = false;
        private int hoveredID = -1;
        private VisualElement root;
        private VisualElement nodeDataContainer;
        private GenericMenu typeMenu;
        private Button typeButton;
        private SceneView sceneView;
        private PathGrid grid;
        private SerializedObject editorSO;
        private SerializedObject gridSO;
        private SerializedProperty nodeDataProp;
        private int zDepth = 0;

        private void OnEnable()
        {
            grid = target as PathGrid;
            editorSO = new SerializedObject(this);
            nodeDataProp = editorSO.FindProperty("nodeData");
            gridSO = new SerializedObject(grid);
            CreateTypeMenu();
        }

        public override void OnActivated()
        {
            isActivated = true;

            sceneView = EditorWindow.GetWindow<SceneView>();

            if (!sceneView.sceneViewState.fxEnabled)
                sceneView.sceneViewState.fxEnabled = true;

            nodeHandles.Clear();
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

            var depthSlider = new SliderInt("Z Depth", 0, grid.GridSize.z - 1)
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

            string typeName = nodeData == null ? "None" : nodeData.GetType().Name;
            typeButton = new Button(typeMenu.ShowAsContext)
            {
                text = typeName
            };
            typeButton.style.marginLeft = 4;
            typeButton.style.flexGrow = 1;

            typeContainer.AddAll(typeLabel, typeButton);

            nodeDataContainer = new VisualElement();
            nodeDataContainer.style.display = DisplayStyle.None;

            root.AddAll(nodeDataContainer, depthSlider, typeContainer);
            sceneView.rootVisualElement.Add(root);

            if (nodeData != null)
                UpdateNodeDataFields();
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
            if (type != null)
                nodeDataProp.boxedValue = (PathNodeData)Activator.CreateInstance(type);
            else
                nodeDataProp.boxedValue = null;

            string typeName = type == null ? "None" : type.Name;
            typeButton.text = typeName;

            editorSO.ApplyModifiedProperties();

            UpdateNodeDataFields();
        }

        private void UpdateNodeDataFields()
        {
            nodeDataContainer.Clear();
            nodeDataContainer.Unbind();

            if (nodeData == null)
                nodeDataContainer.style.display = DisplayStyle.None;
            else
            {
                var defaultFields = VisualElementUtil.CreateDefaultFields(editorSO);

                nodeDataContainer.Add(defaultFields);
                nodeDataContainer.style.display = DisplayStyle.Flex;
            }
        }

        public void OnDrawHandles()
        {
            if (!isActivated)
            {
                var color = Color.white;
                color.a = 0.1f;
                Handles.color = color;

                foreach (var node in grid.Nodes)
                    Handles.DrawWireCube(grid.transform.TransformPoint(node.LocalPosition), grid.NodeSize * Vector3.one);

                return;
            }

            Handles.color = Color.white;

            foreach (var node in grid.Nodes)
            {
                if (node.GridPosition.z == zDepth)
                    CreateNodeHandle(node);
            }
        }

        private void CreateNodeHandle(PathNode node)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Vector3 handlePosition = grid.transform.TransformPoint(node.LocalPosition);
            Vector3 handleSize = grid.NodeSize * 0.98f * Vector3.one;
            Vector3 screenPosition = Handles.matrix.MultiplyPoint(handlePosition);

            if (!nodeHandles.ContainsKey(controlID))
                nodeHandles[controlID] = node;

            Color color;

            if (node.Data != null)
                color = node.Data.EditorColor;
            else
                color = new(1f, 1f, 1f, 0.5f);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCube(screenPosition, Quaternion.identity, grid.NodeSize));

                    break;
                case EventType.MouseDown:
                    isMouseDown = Event.current.button == 0;

                    if (isMouseDown && HandleUtility.nearestControl == controlID)
                    {
                        GUIUtility.hotControl = controlID;
                        PaintHandle(controlID);

                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    isMouseDown = false;
                    hoveredHandles.Clear();
                    break;
                case EventType.Repaint:

                    if (HandleUtility.nearestControl != controlID)
                        break;

                    if (controlID != hoveredID)
                    {
                        hoveredID = controlID;

                        if (isMouseDown)
                            PaintHandle(controlID);
                    }

                    color = Color.yellow;
                    handleSize *= 0.9f;

                    break;
            }

            Handles.color = color;
            Handles.DrawWireCube(handlePosition, handleSize);
        }
    
        private void PaintHandle(int id)
        {
            if (!nodeHandles.TryGetValue(id, out var node))
            {
                Debug.LogError("Could not find node for id: " + id);
                return;
            }

            if (node.Data == null && nodeData == null)
                return;
            else if (hoveredHandles.Contains(id))
                return;

            var nodesProp = gridSO.FindProperty("nodes");
            Vector3Int pos = node.GridPosition;
            int index = (pos.z * grid.GridSize.y * grid.GridSize.x) + (pos.y * grid.GridSize.x) + pos.x;
            var nodeProp = nodesProp.GetArrayElementAtIndex(index);

            var dataProp = nodeProp.FindPropertyRelative("data");

            if (nodeData == null)
                dataProp.boxedValue = null;
            else
                dataProp.boxedValue = nodeData.Clone();

            hoveredHandles.Add(id);
            gridSO.ApplyModifiedProperties();
            return;
        }
    }
}
