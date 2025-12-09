using Shears.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using SearchViewFlags = UnityEngine.Search.SearchViewFlags;

namespace Shears.Pathfinding.Editor
{
    [EditorTool("Path Grid Tool", typeof(PathGrid))]
    public class PathGridEditorTool : EditorTool, IDrawSelectedHandles
    {
        #region Variables
        [SerializeField] private bool drawNodeData = true;
        [SerializeField] private bool drawPrefab = true;
        [SerializeField] private bool drawAllDepths = true;
        [SerializeField] private int zDepth;
        [SerializeReference] private PathNodeData nodeData;
        [SerializeField] private GameObject nodePrefab;

        private readonly Dictionary<int, PathNode> nodeHandles = new();
        private readonly Dictionary<Vector2Int, List<PathNode>> nodeHandleRows = new();
        private readonly List<MenuItem> menuItems = new();
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
        private SerializedProperty zDepthProp;
        private SerializedProperty nodePrefabProp;
        private SerializedProperty drawNodeDataProp;
        private SerializedProperty drawPrefabProp;
        private SerializedProperty drawAllDepthsProp;
        #endregion

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

        private void OnEnable()
        {
            grid = target as PathGrid;
            editorSO = new SerializedObject(this);
            gridSO = new SerializedObject(grid);

            nodeDataProp = editorSO.FindProperty("nodeData");
            zDepthProp = editorSO.FindProperty("zDepth");
            nodePrefabProp = editorSO.FindProperty("nodePrefab");
            drawNodeDataProp = editorSO.FindProperty("drawNodeData");
            drawPrefabProp = editorSO.FindProperty("drawPrefab");
            drawAllDepthsProp = editorSO.FindProperty("drawAllDepths");

            CreateTypeMenu();
        }

        public override void OnActivated()
        {
            isActivated = true;

            sceneView = EditorWindow.GetWindow<SceneView>();

            if (!sceneView.sceneViewState.fxEnabled)
                sceneView.sceneViewState.fxEnabled = true;

            nodeHandles.Clear();
            nodeHandleRows.Clear();

            CreateGUI();
        }

        public override void OnWillBeDeactivated()
        {
            isActivated = false;
            sceneView.rootVisualElement.Remove(root);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.shift)
            {
                if (Event.current.keyCode == KeyCode.Alpha1)
                {
                    zDepthProp.intValue = Mathf.Max(0, zDepth - 1);
                    editorSO.ApplyModifiedProperties();
                }
                else if (Event.current.keyCode == KeyCode.Alpha2)
                {
                    zDepthProp.intValue = Mathf.Min(grid.GridSize.z - 1, zDepth + 1);
                    editorSO.ApplyModifiedProperties();
                }
            }
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
                showInputField = true
            };
            depthSlider.BindProperty(zDepthProp);
            depthSlider.style.marginBottom = 8;
            depthSlider.labelElement.style.minWidth = 80;
            depthSlider.hierarchy[1].hierarchy[0].style.minWidth = 60;

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

            var prefabField = CreateNodePrefabField();

            var drawNodeDataField = new PropertyField(drawNodeDataProp);
            var drawPrefabField = new PropertyField(drawPrefabProp);
            var drawAllDepthsField = new PropertyField(drawAllDepthsProp);

            drawNodeDataField.BindProperty(drawNodeDataProp);
            drawPrefabField.BindProperty(drawPrefabProp);
            drawAllDepthsField.BindProperty(drawAllDepthsProp);

            nodeDataContainer = new VisualElement();
            nodeDataContainer.style.display = DisplayStyle.None;

            root.AddAll(nodeDataContainer, drawNodeDataField, drawPrefabField, drawAllDepthsField, depthSlider, prefabField, typeContainer);
            sceneView.rootVisualElement.Add(root);

            CreateNodeRows();

            if (nodeData != null)
                UpdateNodeDataFields();
        }

        private void CreateNodeRows()
        {
            foreach (var node in grid.Nodes)
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
            if (type != null)
                nodeDataProp.boxedValue = (PathNodeData)Activator.CreateInstance(type);
            else
                nodeDataProp.boxedValue = null;

            string typeName = type == null ? "None" : type.Name;
            typeButton.text = typeName;

            editorSO.ApplyModifiedProperties();

            UpdateNodeDataFields();
        }

        private VisualElement CreateNodePrefabField()
        {
            var searchContext = UnityEditor.Search.SearchService.CreateContext("asset", $"p: t:{nameof(PathNodeObject)}");
            var searchViewFlags = SearchViewFlags.Borderless | SearchViewFlags.DisableInspectorPreview;
            var searchState = new UnityEditor.Search.SearchViewState(searchContext, searchViewFlags);

            var prefabField = new UnityEditor.Search.ObjectField("Node Prefab")
            {
                objectType = typeof(GameObject),
                searchContext = searchContext,
                searchViewFlags = searchViewFlags,
                searchViewState = searchState
            };

            prefabField.BindProperty(nodePrefabProp);

            return prefabField;
        }

        private void UpdateNodeDataFields()
        {
            nodeDataContainer.Clear();
            nodeDataContainer.Unbind();

            if (nodeData == null)
                nodeDataContainer.style.display = DisplayStyle.None;
            else
            {
                var dataField = new PropertyField(nodeDataProp);
                dataField.Bind(editorSO);

                nodeDataContainer.Add(dataField);
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
                {
                    var worldPosition = GetWorldPosition(node);

                    Handles.DrawWireCube(worldPosition, grid.NodeSize * Vector3.one);
                }

                return;
            }

            Handles.color = Color.white;
            nodeHandles.Clear();

            foreach (var node in grid.Nodes)
            {
                if (node.GridPosition.z == zDepth)
                    CreateNodeHandle(node);
            }
        }

        private void CreateNodeHandle(PathNode node)
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Vector3 handlePosition = GetWorldPosition(node);
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

            node.Data?.DrawHandles(handlePosition, grid.NodeSize);
        }

        private void PaintHandle(int id)
        {
            if (hoveredHandles.Contains(id))
                return;

            if (!nodeHandles.TryGetValue(id, out var node))
            {
                Debug.LogError("Could not find node for id: " + id);
                return;
            }

            if (drawAllDepths)
            {
                Vector2Int flatPosition = (Vector2Int)node.GridPosition.XY();

                if (!nodeHandleRows.TryGetValue(flatPosition, out var row))
                {
                    Debug.LogError("Could not find node row for id: " + id);
                    return;
                }

                gridSO.Update();
                foreach (var rowNode in row)
                {
                    Vector3Int pos = rowNode.GridPosition;
                    int index = (pos.z * grid.GridSize.y * grid.GridSize.x) + (pos.y * grid.GridSize.x) + pos.x;

                    var nodesProp = gridSO.FindProperty("nodes");
                    var nodeProp = nodesProp.GetArrayElementAtIndex(index);

                    PaintNode(rowNode, nodeProp);
                }

                hoveredHandles.Add(id);
                gridSO.ApplyModifiedProperties();
            }
            else
            {
                gridSO.Update();
                Vector3Int pos = node.GridPosition;
                int index = (pos.z * grid.GridSize.y * grid.GridSize.x) + (pos.y * grid.GridSize.x) + pos.x;

                var nodesProp = gridSO.FindProperty("nodes");
                var nodeProp = nodesProp.GetArrayElementAtIndex(index);

                PaintNode(node, nodeProp);

                hoveredHandles.Add(id);
                gridSO.ApplyModifiedProperties();
            }
        }

        private void PaintNode(PathNode node, SerializedProperty nodeProp)
        {
            if (drawNodeData && (node.Data != null || nodeData != null))
            {
                var dataProp = nodeProp.FindPropertyRelative("data");

                if (nodeData == null)
                    dataProp.boxedValue = null;
                else
                    dataProp.boxedValue = nodeData.Clone();
            }

            if (drawPrefab)
                PaintPrefab(node, nodeProp);
        }

        private void PaintPrefab(PathNode node, SerializedProperty nodeProp)
        {
            var nodeObjectProp = nodeProp.FindPropertyRelative("nodeObject");

            if (nodeObjectProp.objectReferenceValue != null)
                Undo.DestroyObjectImmediate(nodeObjectProp.objectReferenceValue);

            if (nodePrefab == null) // if we are placing nothing, just clear the object value
            {
                nodeObjectProp.objectReferenceValue = null;
                return;
            }

            var newInstance = PrefabUtility.InstantiatePrefab(nodePrefab, grid.transform) as GameObject;
            Undo.RegisterCreatedObjectUndo(newInstance, "Instantiate Node Prefab");
            newInstance.transform.position = GetWorldPosition(node);

            nodeObjectProp.objectReferenceValue = newInstance;
        }

        private Vector3 GetWorldPosition(PathNode node)
        {
            return grid.transform.TransformPoint(grid.NodeSize * (Vector3)node.GridPosition);
        }
    }
}
