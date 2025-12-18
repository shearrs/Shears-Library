using Shears.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Search;
using UnityEngine.UIElements;

namespace Shears.Pathfinding.Editor
{
    public class PGETUI : VisualElement
    {
        private readonly PGETSettings settings;
        private readonly Dictionary<int, PathNode> nodeHandles = new();
        private readonly Dictionary<Vector2Int, List<PathNode>> nodeHandleRows = new();
        private readonly List<MenuItem> menuItems = new();
        private readonly List<int> hoveredHandles = new();

        private bool isMouseDown = false;
        private int hoveredID = -1;
        private VisualElement nodeDataContainer;
        private GenericMenu typeMenu;
        private Button typeButton;

        private PathGrid Grid => settings.Grid;

        public event Action<Type> TypeSelected;
        public event Action<PathNode, SerializedProperty> PaintRequested;

        private readonly struct MenuItem
        {
            private readonly string name;
            private readonly int order;
            private readonly Type type;

            public readonly string MenuPath => name;
            public readonly int Order => order;
            public readonly Type Type => type;

            public MenuItem(string name, int order, Type type)
            {
                this.name = name;
                this.order = order;
                this.type = type;
            }
        }

        public PGETUI(PGETSettings settings)
        {
            this.settings = settings;

            CreateTypeMenu();
            CreateUI();
        }

        private void CreateUI()
        {
            var root = new VisualElement();
            root.SetAllPadding(4);
            root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            root.SetAllBorderColors(new Color(0.1f, 0.1f, 0.1f, 0.8f));
            root.SetAllBorders(1);

            var depthSlider = new SliderInt("Z Depth", 0, Grid.GridSize.z - 1)
            {
                showInputField = true
            };
            depthSlider.BindProperty(settings.ZDepthProp);
            depthSlider.style.marginBottom = 8;
            depthSlider.labelElement.style.minWidth = 80;
            depthSlider.hierarchy[1].hierarchy[0].style.minWidth = 60;

            var typeContainer = new VisualElement();
            typeContainer.style.flexDirection = FlexDirection.Row;

            var typeLabel = new Label("Data Type");

            string typeName = settings.NodeData == null ? "None" : settings.NodeData.GetType().Name;
            typeButton = new Button(typeMenu.ShowAsContext)
            {
                text = typeName
            };
            typeButton.style.marginLeft = 4;
            typeButton.style.flexGrow = 1;

            typeContainer.AddAll(typeLabel, typeButton);

            var prefabField = CreateNodePrefabField();

            var drawNodeDataField = new PropertyField(settings.DrawNodeDataProp);
            var drawPrefabField = new PropertyField(settings.DrawPrefabProp);
            var drawAllDepthsField = new PropertyField(settings.DrawAllDepthsProp);

            drawNodeDataField.BindProperty(settings.DrawNodeDataProp);
            drawPrefabField.BindProperty(settings.DrawPrefabProp);
            drawAllDepthsField.BindProperty(settings.DrawAllDepthsProp);

            nodeDataContainer = new VisualElement();
            nodeDataContainer.style.display = DisplayStyle.None;

            root.AddAll(nodeDataContainer, drawNodeDataField, drawPrefabField, drawAllDepthsField, depthSlider, prefabField, typeContainer);
            Add(root);

            CreateNodeRows();

            if (settings.NodeData != null)
                UpdateNodeDataFields();
        }

        private void CreateTypeMenu()
        {
            typeMenu = new GenericMenu();

            typeMenu.AddItem(new("None"), false, () => OnTypeSelected(null));
            menuItems.Clear();

            foreach (var type in TypeCache.GetTypesDerivedFrom<PathNodeData>())
            {
                if (!type.IsAbstract)
                {
                    var attribute = type.GetCustomAttribute<NodeDataMenuItem>();
                    MenuItem menuItem;

                    if (attribute == null)
                        menuItem = new(type.Name, int.MaxValue, type);
                    else
                        menuItem = new(attribute.MenuPath, attribute.Order, type);

                    menuItems.Add(menuItem);
                }
            }

            menuItems.Sort((item1, item2) => item2.Order.CompareTo(item1.Order));

            foreach (var item in menuItems)
                typeMenu.AddItem(new(item.MenuPath), false, () => OnTypeSelected(item.Type));
        }

        private void OnTypeSelected(Type type)
        {
            TypeSelected?.Invoke(type);

            UpdateNodeData();
        }

        private void UpdateNodeData()
        {
            var type = settings.NodeData?.GetType();

            string typeName = type == null ? "None" : type.Name;
            typeButton.text = typeName;

            UpdateNodeDataFields();
        }

        private VisualElement CreateNodePrefabField()
        {
            var searchContext = UnityEditor.Search.SearchService.CreateContext("asset", $"p: t:{nameof(PathNodeObject)} -name:_Base_");
            var searchViewFlags = SearchViewFlags.Borderless | SearchViewFlags.DisableInspectorPreview;
            var searchState = new UnityEditor.Search.SearchViewState(searchContext, searchViewFlags);

            var prefabField = new UnityEditor.Search.ObjectField("Node Prefab")
            {
                objectType = typeof(GameObject),
                searchContext = searchContext,
                searchViewFlags = searchViewFlags,
                searchViewState = searchState
            };

            prefabField.BindProperty(settings.NodePrefabProp);

            return prefabField;
        }

        private void UpdateNodeDataFields()
        {
            nodeDataContainer.Clear();
            nodeDataContainer.Unbind();

            if (settings.NodeData == null)
                nodeDataContainer.style.display = DisplayStyle.None;
            else
            {
                var dataField = new PropertyField(settings.NodeDataProp);
                dataField.Bind(settings.EditorSO);

                nodeDataContainer.Add(dataField);
                nodeDataContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void CreateNodeRows()
        {
            foreach (var node in Grid.Nodes)
            {
                Vector2Int flatPosition = (Vector2Int)node.GridPosition.XY();

                if (nodeHandleRows.TryGetValue(flatPosition, out var row))
                {
                    if (!row.Contains(node))
                        row.Add(node);
                }
                else
                {
                    var list = new List<PathNode>
                    {
                        node
                    };

                    nodeHandleRows[flatPosition] = list;
                }
            }
        }

        public void DrawHandles()
        {
            if (!settings.IsActivated)
            {
                var color = Color.white;
                color.a = 0.1f;
                Handles.color = color;

                foreach (var node in Grid.Nodes)
                {
                    var worldPosition = PathGridEditorTool.GetWorldPosition(Grid, node);

                    Handles.DrawWireCube(worldPosition, Grid.NodeSize * Vector3.one);
                }

                return;
            }

            Handles.color = Color.white;
            nodeHandles.Clear();

            foreach (var node in Grid.Nodes)
            {
                if (node.GridPosition.z == settings.ZDepth)
                    CreateNodeHandle(node);
            }
        }

        private void CreateNodeHandle(PathNode node)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Vector3 handlePosition = PathGridEditorTool.GetWorldPosition(Grid, node);
            Vector3 handleSize = Grid.NodeSize * 0.98f * Vector3.one;
            Vector3 screenPosition = Handles.matrix.MultiplyPoint(handlePosition);

            if (!nodeHandles.ContainsKey(controlID))
                nodeHandles[controlID] = node;

            Color color;

            if (settings.DrawNodeData && node.Data != null)
                color = node.Data.EditorColor;
            else
                color = new(1f, 1f, 1f, 0.5f);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCube(screenPosition, Quaternion.identity, Grid.NodeSize));

                    break;
                case EventType.MouseDown:
                    isMouseDown = Event.current.button == 0;

                    if (isMouseDown && HandleUtility.nearestControl == controlID)
                    {
                        GUIUtility.hotControl = controlID;
                        RequestPaint(controlID);

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
                            RequestPaint(controlID);
                    }

                    color = Color.yellow;
                    handleSize *= 0.9f;

                    break;
            }

            Handles.color = color;
            Handles.DrawWireCube(handlePosition, handleSize);

            if (settings.DrawNodeData)
                node.Data?.DrawHandles(handlePosition, Grid.NodeSize);
        }

        private void RequestPaint(int controlID)
        {
            if (hoveredHandles.Contains(controlID))
                return;

            if (!nodeHandles.TryGetValue(controlID, out var node))
            {
                Debug.LogError("Could not find node for id: " + controlID);
                return;
            }

            if (settings.DrawAllDepths)
            {
                Vector2Int flatPosition = (Vector2Int)node.GridPosition.XY();

                if (!nodeHandleRows.TryGetValue(flatPosition, out var row))
                {
                    Debug.LogError("Could not find node row for id: " + controlID);
                    return;
                }

                foreach (var rowNode in row)
                {
                    Vector3Int pos = rowNode.GridPosition;
                    int index = (pos.z * Grid.GridSize.y * Grid.GridSize.x) + (pos.y * Grid.GridSize.x) + pos.x;

                    var nodesProp = settings.GridSO.FindProperty("nodes");
                    var nodeProp = nodesProp.GetArrayElementAtIndex(index);

                    PaintRequested?.Invoke(rowNode, nodeProp);
                }

                hoveredHandles.Add(controlID);
            }
            else
            {
                Vector3Int pos = node.GridPosition;
                int index = (pos.z * Grid.GridSize.y * Grid.GridSize.x) + (pos.y * Grid.GridSize.x) + pos.x;

                var nodesProp = settings.GridSO.FindProperty("nodes");
                var nodeProp = nodesProp.GetArrayElementAtIndex(index);

                PaintRequested?.Invoke(node, nodeProp);

                hoveredHandles.Add(controlID);
            }
        }
    }
}
