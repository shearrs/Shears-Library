using Shears.Editor;
using Shears.Logging;
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
        private readonly List<ISelectable> instanceSelection = new();
        private readonly List<GraphNode> instanceNodes = new();
        private readonly List<GraphEdge> instanceEdges = new();
        private GraphData graphData;
        private VisualElement rootContainer; // holds everything
        private VisualElement bodyContainer; // holds graph container and possible bars
        private VisualElement graphViewContainer; // holds background and content
        private VisualElement contentViewContainer; // holds things in the graph
        private GridBackground gridBackground;
        private ElementTransform viewTransform;

        protected VisualElement RootContainer => rootContainer;
        protected VisualElement BodyContainer => bodyContainer;
        protected VisualElement GraphViewContainer => graphViewContainer;
        public VisualElement ContentViewContainer => contentViewContainer;
        public ElementTransform ViewTransform => viewTransform;
        public int SelectionCount => graphData.SelectionCount;

        public event Action NodesCleared;
        public event Action EdgesCleared;
        public event Action<GraphData> GraphDataSet;
        public event Action GraphDataCleared;
        #endregion

        public class ElementTransform
        {
            private readonly VisualElement element;

            public Vector3 Position { get => element.resolvedStyle.translate; set => element.style.translate = new Translate(value.x, value.y); }
            public Vector3 Scale { get => element.resolvedStyle.scale.value; set => element.style.scale = new Scale(value); }
            public Quaternion Rotation
            {
                get => Quaternion.Euler(0, 0, element.resolvedStyle.rotate.angle.value); set
                {
                    value.ToAngleAxis(out float angle, out Vector3 _);

                    element.style.rotate = new Rotate(angle);
                }
            }

            public ElementTransform(VisualElement element) => this.element = element;
        }

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
            CreateBodyContainer();
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
                graphData.NodeDataAddedToLayer -= AddNodeFromData;
                graphData.NodeDataRemoved -= RemoveNodeFromData;
            }
        }

        private void OnUndoRedo()
        {
            ReloadLayer();
        }

        public void Record(string undoName = "Graph View Undo")
        {
            GraphViewEditorUtil.Record(graphData, undoName);
        }

        public void Save()
        {
            GraphViewEditorUtil.Save(graphData);
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
            graphData.NodeDataAddedToLayer += AddNodeFromData;
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
            graphData.NodeDataAddedToLayer -= AddNodeFromData;
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

        private void CreateBodyContainer()
        {
            bodyContainer = new VisualElement
            {
                name = "bodyContainer"
            };

            bodyContainer.AddToClassList(GraphViewEditorUtil.BodyContainerClassName);
            rootContainer.Add(bodyContainer);
        }

        private void CreateGraphViewContainer()
        {
            graphViewContainer = new()
            {
                name = "graphViewContainer"
            };

            graphViewContainer.AddToClassList(GraphViewEditorUtil.GraphViewContainerClassName);
            bodyContainer.Add(graphViewContainer);
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
            viewTransform = new(contentViewContainer);

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

            bool hasSelection = GetSelection().Count > 0;

            if (hasSelection && evt.keyCode == KeyCode.Delete)
                DeleteSelection();
            else if (hasSelection && evt.keyCode == KeyCode.F)
                FocusCamera(GetSelection());
            else if (hasSelection && evt.keyCode == KeyCode.Return)
                TryOpenSelection();
            else if (hasSelection && evt.keyCode == KeyCode.C && evt.modifiers.HasFlag(EventModifiers.Control))
                CopySelectionToClipboard();
            else if (hasSelection && evt.keyCode == KeyCode.X && evt.modifiers.HasFlag(EventModifiers.Control))
                CutSelectionToClipboard();
            else if (evt.keyCode == KeyCode.V && evt.modifiers.HasFlag(EventModifiers.Control))
                PasteFromClipboard();
            else if (evt.keyCode == KeyCode.A)
                FocusCamera(nodes.Values);
        }

        private void CopySelectionToClipboard()
        {
            graphData.CopySelectionToClipboard();
            Save();
        }

        private void CutSelectionToClipboard()
        {
            graphData.CopySelectionToClipboard();
            DeleteSelection();
            Save();
        }

        private void PasteFromClipboard()
        {
            Record("Paste From Clipboard");
            graphData.PasteFromClipboard();
            Save();
        }

        protected void DeleteSelection()
        {
            GraphViewEditorUtil.Record(graphData, "Delete Selection");

            graphData.DeleteSelection();

            GraphViewEditorUtil.Save(graphData);
        }

        private void FocusCamera(IReadOnlyCollection<IGraphElement> targets)
        {
            Vector2 averagePosition = Vector2.zero;
            int nodes = 0;

            foreach (var selectable in targets)
            {
                if (selectable is GraphNode node)
                {
                    Vector2 center = node.resolvedStyle.translate;
                    center.x += node.layout.width / 2;
                    center.y += node.layout.height / 2;

                    averagePosition -= center;
                    nodes++;
                }
            }

            if (nodes == 0)
                return;

            averagePosition /= nodes;
            averagePosition *= ViewTransform.Scale;
            averagePosition.x += graphViewContainer.layout.width * 0.5f;
            averagePosition.y += graphViewContainer.layout.height * 0.5f;

            UpdateViewTransform(averagePosition, ViewTransform.Scale);
            SaveViewTransform();
        }

        private void TryOpenSelection()
        {
            var selection = GetSelection();

            if (selection.Count != 1)
                return;

            if (selection[0] is GraphMultiNode multiNode)
                graphData.OpenLayer(new(multiNode.Data));
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
            const float MAX_DISTANCE = 5000.0f;
            float maxDistance = MAX_DISTANCE * ViewTransform.Scale.x;

            newPosition = newPosition.ClampComponents(-maxDistance, maxDistance);

            newPosition.x = EditorGUIHelper.RoundToPixelGrid(newPosition.x);
            newPosition.y = EditorGUIHelper.RoundToPixelGrid(newPosition.y);

            ViewTransform.Position = newPosition;
            ViewTransform.Scale = new Vector3(newScale.x, newScale.y, 1);
        }

        public void SaveViewTransform()
        {
            graphData.Position = ViewTransform.Position;
            graphData.Scale = ViewTransform.Scale;
        }
        #endregion

        #region Nodes
        public GraphNode GetNode(string id)
        {
            if (!graphData.TryGetData(id, out GraphNodeData data))
            {
                SHLogger.Log("Could not find node data for id: " + id, SHLogLevels.Error);
                return null;
            }

            return GetNode(data);
        }

        // TODO: should be TryGetNode
        public GraphNode GetNode(GraphNodeData nodeData)
        {
            if (nodes.ContainsKey(nodeData))
                return nodes[nodeData];

            return null;
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

        protected abstract GraphNode CreateNodeFromData(GraphNodeData data);
        #endregion

        #region Edges
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

            if (edge == null)
                return;

            AddEdge(edge);
        }

        private void RemoveEdgeFromData(GraphEdgeData data)
        {
            if (edges.TryGetValue(data, out var edge))
                RemoveEdge(edge);
        }

        protected abstract GraphEdge CreateEdgeFromData(GraphEdgeData data);

        public GraphEdge GetEdge(string id)
        {
            if (!graphData.TryGetData(id, out GraphEdgeData data))
            {
                SHLogger.Log("Could not find edge data for id: " + id, SHLogLevels.Error);
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
        public void SelectAll(IReadOnlyCollection<ISelectable> selectables)
        {
            Select(null);

            foreach (var element in selectables)
                Select(element, true);
        }

        public void Select(ISelectable selectable, bool isMultiSelect = false)
        {
            if (graphData == null)
                return;

            if (selectable == null)
                graphData.Select(null);
            else
            {
                Selection.activeObject = graphData;

                graphData.Select(selectable.GetData(), isMultiSelect);
            }
        }

        protected IReadOnlyList<ISelectable> GetSelection()
        {
            instanceSelection.Clear();

            if (graphData == null)
                return instanceSelection;

            var selectionData = graphData.GetSelection();

            foreach (var data in selectionData)
            {
                if (nodes.TryGetValue(data, out var node))
                    instanceSelection.Add(node);
                else if (edges.TryGetValue(data, out var edge))
                    instanceSelection.Add(edge);
            }

            var extraSelection = GetExtraSelection();

            if (extraSelection != null)
                instanceSelection.AddRange(extraSelection);

            return instanceSelection;
        }

        protected virtual IReadOnlyList<ISelectable> GetExtraSelection() => null;
        #endregion
    }
}
