using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphView : VisualElement
    {
        #region Variables
        private readonly ContentDragger contentDragger;
        private readonly ContentZoomer contentZoomer;
        private readonly ContentSelector contentSelector;
        private readonly MultiNodeSelector multiNodeSelector;
        private readonly NodeDragger nodeDragger;
        private readonly RectangleSelector rectangleSelector;
        private readonly EdgePlacer edgePlacer;
        private readonly Dictionary<GraphElementData, GraphNode> nodes = new();
        private readonly Dictionary<GraphElementData, GraphEdge> edges = new();
        private readonly List<GraphElement> instanceElements = new();
        private readonly List<GraphNode> instanceNodes = new();
        private readonly List<GraphEdge> instanceEdges = new();
        private GraphData graphData;
        private VisualElement rootContainer;
        private VisualElement graphViewContainer;
        private VisualElement contentViewContainer;
        private GridBackground gridBackground;
        
        protected VisualElement RootContainer => rootContainer;
        public VisualElement GraphViewContainer => graphViewContainer;
        public VisualElement ContentViewContainer => contentViewContainer;
        public ITransform ViewTransform => contentViewContainer.transform;
        public int SelectionCount => graphData.SelectionCount;

        public event Action NodesCleared;
        public event Action EdgesCleared;
        public event Action<GraphData> GraphDataSet;
        public event Action GraphDataCleared;
        #endregion

        protected GraphView()
        {
            this.AddStyleSheet(GraphViewEditorUtil.GraphViewStyleSheet);
            AddToClassList(GraphViewEditorUtil.GraphViewClassName);

            focusable = true;
            contentDragger = new(this);
            contentZoomer = new(this);
            contentSelector = new(this);
            multiNodeSelector = new();
            nodeDragger = new(this);
            rectangleSelector = new(this);
            edgePlacer = new(this);

            edgePlacer.PlacingBegan += OnPlacingBegin;
            edgePlacer.PlacingEnded += OnPlacingEnd;

            CreateRootContainer();
            CreateGraphViewContainer();
            CreateContentViewContainer();
            schedule.Execute(() => SetGraphData(GraphEditorState.instance.GraphData)).StartingIn(1);

            GraphViewEditorUtil.UndoRedoEvent += OnUndoRedo;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        ~GraphView()
        {
            if (graphData != null)
            {
                graphData.LayersChanged -= ReloadLayer;
                graphData.NodeDataAdded -= AddNodeFromData;
                graphData.NodeDataRemoved -= RemoveNodeFromData;
            }
        }

        private void OnUndoRedo()
        {
            ReloadLayer();
        }

        #region Initialization
        public void SetGraphData(GraphData graphData)
        {
            if (graphData == this.graphData || graphData == null)
                return;
            else if (this.graphData != null)
                ClearGraphData();

            this.graphData = graphData;
            multiNodeSelector.SetGraphData(graphData);
            nodeDragger.SetGraphData(graphData);

            graphData.LayersChanged += ReloadLayer;
            graphData.NodeDataAdded += AddNodeFromData;
            graphData.NodeDataRemoved += RemoveNodeFromData;
            graphData.EdgeDataAdded += AddEdgeFromData;
            graphData.EdgeDataRemoved += RemoveEdgeFromData;

            CreateBackground();
            AddManipulators();
            UpdateViewTransform(graphData.Position, graphData.Scale);

            GraphEditorState.instance.SetGraphData(graphData);
            GraphDataSet?.Invoke(graphData);

            LoadGraphData();
            Select(null);
        }

        public void ClearGraphData()
        {
            if (graphData == null) 
                return; 

            graphData.LayersChanged -= ReloadLayer;
            graphData.NodeDataAdded -= AddNodeFromData;
            graphData.NodeDataRemoved -= RemoveNodeFromData;
            graphData.EdgeDataAdded -= AddEdgeFromData;
            graphData.EdgeDataRemoved -= RemoveEdgeFromData;
            graphData = null;

            nodes.Clear();
            edges.Clear();
            contentViewContainer.Clear();
            graphViewContainer.Remove(gridBackground);
            RemoveManipulators();

            GraphEditorState.instance.SetGraphData(null);
            GraphDataCleared?.Invoke();
        }

        protected void ReloadLayer()
        {
            Select(null);
            ClearEdges();
            ClearNodes();
            LoadNodes();
            LoadEdges();
        }

        private void CreateRootContainer()
        {
            rootContainer = new VisualElement
            {
                name = "rootContainer"
            };

            rootContainer.AddToClassList(GraphViewEditorUtil.RootContainerClassName);
            Add(rootContainer);
        }

        private void CreateGraphViewContainer()
        {
            graphViewContainer = new()
            {
                name = "graphViewContainer"
            };

            graphViewContainer.AddToClassList(GraphViewEditorUtil.GraphViewContainerClassName);
            rootContainer.Add(graphViewContainer);
        }

        private void CreateContentViewContainer()
        {
            contentViewContainer = new()
            {
                name = "contentViewContainer",
                pickingMode = PickingMode.Ignore
            };

            contentViewContainer.AddToClassList(GraphViewEditorUtil.ContentViewContainerClassName);
            contentViewContainer.style.top = 0;
            contentViewContainer.style.left = 0;

            graphViewContainer.Add(contentViewContainer);
        }

        private void CreateBackground()
        {
            gridBackground = new GridBackground(this);

            graphViewContainer.Insert(0, gridBackground);
        }

        private void AddManipulators()
        {
            graphViewContainer.AddManipulator(contentDragger);
            graphViewContainer.AddManipulator(contentZoomer);
            graphViewContainer.AddManipulator(contentSelector);
            graphViewContainer.AddManipulator(multiNodeSelector);
            graphViewContainer.AddManipulator(nodeDragger);
            graphViewContainer.AddManipulator(rectangleSelector);
            graphViewContainer.AddManipulator(edgePlacer);
        }

        private void RemoveManipulators()
        {
            graphViewContainer.RemoveManipulator(contentDragger);
            graphViewContainer.RemoveManipulator(contentZoomer);
            graphViewContainer.RemoveManipulator(contentSelector);
            graphViewContainer.RemoveManipulator(multiNodeSelector);
            graphViewContainer.RemoveManipulator(nodeDragger);
            graphViewContainer.RemoveManipulator(rectangleSelector);
            graphViewContainer.RemoveManipulator(edgePlacer);
        }
        #endregion

        #region Keybinds
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.R && evt.modifiers.HasFlag(EventModifiers.Shift) && evt.modifiers.HasFlag(EventModifiers.Control))
            {
                ClearGraphData();
                return;
            }

            if (graphData == null)
                return;

            bool hasSelection = graphData.GetSelection().Count > 0;

            if (hasSelection && evt.keyCode == KeyCode.Delete)
                DeleteSelection();
            else if (hasSelection && evt.keyCode == KeyCode.F)
                FocusCamera(GetSelection());
            else if (evt.keyCode == KeyCode.A)
                FocusCamera(nodes.Values);
        }

        protected void DeleteSelection()
        {
            GraphViewEditorUtil.Record(graphData, "Delete Selection");

            graphData.DeleteSelection();

            GraphViewEditorUtil.Save(graphData);
        }

        private void FocusCamera(IReadOnlyCollection<GraphElement> targets)
        {
            Vector2 averagePosition = Vector2.zero;
            int nodes = 0;

            foreach (var selectable in targets)
            {
                if (selectable is GraphNode node)
                {
                    Vector2 center = node.transform.position;
                    center.x += node.layout.width / 2;
                    center.y += node.layout.height / 2;

                    averagePosition -= center;
                    nodes++;
                }
            }

            if (nodes == 0)
                return;

            averagePosition /= nodes;
            averagePosition *= ViewTransform.scale;
            averagePosition.x += layout.width * 0.5f;
            averagePosition.y += layout.height * 0.5f;

            UpdateViewTransform(averagePosition, ViewTransform.scale);
            SaveViewTransform();
        }
        #endregion

        #region Loading
        private void LoadGraphData()
        {
            LoadNodes();
            LoadEdges();
        }
        
        private void LoadNodes()
        {
            foreach (var nodeData in graphData.GetActiveNodes())
                AddNodeFromData(nodeData);
        }

        private void LoadEdges()
        {
            foreach (var nodeData in graphData.GetActiveNodes())
            {
                foreach (var edgeID in nodeData.Edges)
                {
                    if (!graphData.TryGetData(edgeID, out GraphEdgeData edgeData))
                        continue;
                    else if (edges.ContainsKey(edgeData))
                        continue;

                    AddEdgeFromData(edgeData);
                }
            }
        }

        protected void ClearNodes()
        {
            instanceNodes.Clear();
            instanceNodes.AddRange(nodes.Values);

            foreach (var node in instanceNodes)
                RemoveNode(node);

            NodesCleared?.Invoke();
        }

        protected void ClearEdges()
        {
            instanceEdges.Clear();
            instanceEdges.AddRange(edges.Values);

            foreach (var edge in instanceEdges)
                RemoveEdge(edge);

            EdgesCleared?.Invoke();
        }
        #endregion

        #region Transformations
        public void UpdateViewTransform(Vector2 newPosition, Vector2 newScale)
        {
            newPosition.x = EditorGUIHelper.RoundToPixelGrid(newPosition.x);
            newPosition.y = EditorGUIHelper.RoundToPixelGrid(newPosition.y);

            ViewTransform.position = newPosition;
            ViewTransform.scale = new Vector3(newScale.x, newScale.y, 1);
        }

        public void SaveViewTransform()
        {
            graphData.Position = ViewTransform.position;
            graphData.Scale = ViewTransform.scale;
        }
        #endregion

        #region Nodes
        public GraphNode GetNode(string id)
        {
            if (!graphData.TryGetData(id, out GraphNodeData data))
            {
                Debug.LogError("Could not find node data for id: " + id);
                return null;
            }

            return GetNode(data);
        }

        public GraphNode GetNode(GraphNodeData nodeData)
        {
            return nodes[nodeData];
        }

        private void AddNode(GraphNode node)
        {
            nodes.Add(node.GetData(), node);

            contentViewContainer.Add(node);
        }

        private void RemoveNode(GraphNode node)
        {
            nodes.Remove(node.GetData());

            if (contentViewContainer.Contains(node))
                contentViewContainer.Remove(node);
        }

        private void AddNodeFromData(GraphNodeData data)
        {
            var node = CreateNodeFromData(data);

            AddNode(node);
        }

        private void RemoveNodeFromData(GraphNodeData data)
        {
            if (nodes.TryGetValue(data, out var node))
                RemoveNode(node);
        }

        public void AddEdge(GraphEdge edge)
        {
            edges.Add(edge.GetData(), edge);

            contentViewContainer.Add(edge);
            edge.SendToBack();
        }

        public void RemoveEdge(GraphEdge edge)
        {
            edges.Remove(edge.GetData());

            if (contentViewContainer.Contains(edge))
                contentViewContainer.Remove(edge);
        }

        private void AddEdgeFromData(GraphEdgeData data)
        {
            var edge = CreateEdgeFromData(data);

            AddEdge(edge);
        }

        private void RemoveEdgeFromData(GraphEdgeData data)
        {
            if (edges.TryGetValue(data, out var edge))
                RemoveEdge(edge);
        }

        protected abstract GraphNode CreateNodeFromData(GraphNodeData data);
        protected abstract GraphEdge CreateEdgeFromData(GraphEdgeData data);
        #endregion

        #region Edges
        public GraphEdge GetEdge(string id)
        {
            if (!graphData.TryGetData(id, out GraphEdgeData data))
            {
                Debug.LogError("Could not find edge data for id: " + id);
                return null;
            }

            return GetEdge(data);
        }

        public GraphEdge GetEdge(GraphEdgeData edgeData)
        {
            return edges[edgeData];
        }

        protected void BeginPlacingEdge(IEdgeAnchorable anchor, Action<IEdgeAnchorable, IEdgeAnchorable> tryPlaceCallback)
        {
            edgePlacer.BeginPlacing(anchor);
            edgePlacer.TryPlaceEdgeCallback = tryPlaceCallback;
        }

        protected void EndPlacingEdge()
        {
            edgePlacer.EndPlacing();
        }

        private void OnPlacingBegin()
        {
            this.RemoveManipulator(contentSelector);
            this.RemoveManipulator(multiNodeSelector);
            this.RemoveManipulator(nodeDragger);
            this.RemoveManipulator(rectangleSelector);
        }

        private void OnPlacingEnd()
        {
            graphViewContainer.AddManipulator(contentSelector);
            graphViewContainer.AddManipulator(multiNodeSelector);
            graphViewContainer.AddManipulator(nodeDragger);
            graphViewContainer.AddManipulator(rectangleSelector);
        }
        #endregion

        public IReadOnlyList<GraphElement> GetElements()
        {
            instanceElements.Clear();

            instanceElements.AddRange(nodes.Values);

            return instanceElements;
        }

        #region Selection
        public void SelectAll(IReadOnlyCollection<GraphElement> elements)
        {
            Select(null);

            foreach (var element in elements)
                Select(element, true);
        }

        public void Select(GraphElement element, bool isMultiSelect = false)
        {
            if (element == null)
                graphData.Select(null);
            else
            {
                Selection.activeObject = graphData;
                graphData.Select(element.GetData(), isMultiSelect);
            }
        }
    
        protected IReadOnlyList<GraphElement> GetSelection()
        {
            var selectionData = graphData.GetSelection();
            instanceElements.Clear();

            foreach (var data in selectionData)
            {
                if (nodes.TryGetValue(data, out var node))
                    instanceElements.Add(node);
                else if (edges.TryGetValue(data, out var edge))
                    instanceElements.Add(edge);
            }

            return instanceElements;
        }
        #endregion
    }
}
