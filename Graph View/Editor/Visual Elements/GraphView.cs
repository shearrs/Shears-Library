using System;
using System.Collections.Generic;
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
        private readonly Dictionary<GraphElementData, GraphNode> nodes = new();
        private readonly List<GraphElement> instanceElements = new();
        private readonly List<GraphNode> instanceNodes = new();
        private GraphData graphData;
        private VisualElement rootContainer;
        private VisualElement graphViewContainer;
        private VisualElement contentViewContainer;
        private GridBackground gridBackground;

        protected VisualElement RootContainer => rootContainer;
        public VisualElement GraphViewContainer => graphViewContainer;
        public VisualElement ContentViewContainer => contentViewContainer;
        public ITransform ViewTransform => contentViewContainer.transform;

        public event Action NodesCleared;
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
            multiNodeSelector = new(this);
            nodeDragger = new(this); 

            CreateRootContainer();
            CreateGraphViewContainer();
            CreateContentViewContainer();
            schedule.Execute(() => SetGraphData(GraphEditorState.instance.GraphData)).StartingIn(1);
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

        #region Initialization
        protected void SetGraphData(GraphData graphData)
        {
            if (graphData == this.graphData || graphData == null)
                return;

            this.graphData = graphData;
            multiNodeSelector.SetGraphData(graphData);
            nodeDragger.SetGraphData(graphData);

            graphData.LayersChanged += ReloadLayer;
            graphData.NodeDataAdded += AddNodeFromData;
            graphData.NodeDataRemoved += RemoveNodeFromData;

            CreateBackground();
            AddManipulators();
            UpdateViewTransform(graphData.Position, graphData.Scale);

            GraphEditorState.instance.SetGraphData(graphData);
            GraphDataSet?.Invoke(graphData);

            LoadGraphData();
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected void ClearGraphData()
        {
            if (graphData == null) 
                return; 

            graphData.LayersChanged -= ReloadLayer;
            graphData.NodeDataAdded -= AddNodeFromData;
            graphData.NodeDataRemoved -= RemoveNodeFromData;
            graphData = null;

            nodes.Clear();
            contentViewContainer.Clear();
            graphViewContainer.Remove(gridBackground);
            graphViewContainer.RemoveManipulator(contentDragger);
            graphViewContainer.RemoveManipulator(contentZoomer);
            graphViewContainer.RemoveManipulator(contentSelector);
            graphViewContainer.RemoveManipulator(multiNodeSelector);
            graphViewContainer.RemoveManipulator(nodeDragger);

            GraphEditorState.instance.SetGraphData(null);
            GraphDataCleared?.Invoke();
        }

        protected void ReloadLayer()
        {
            Select(null);
            ClearNodes();
            LoadNodes();
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
        }
        #endregion

        private void OnKeyDown(KeyDownEvent evt)
        {
            bool hasSelection = graphData.GetSelection().Count > 0;

            if (hasSelection && evt.keyCode == KeyCode.Delete)
                DeleteSelection();
            else if (hasSelection && evt.keyCode == KeyCode.F)
                FocusCamera(GetSelection());
            else if (evt.keyCode == KeyCode.A)
                FocusCamera(nodes.Values);
        }

        private void DeleteSelection()
        {
            graphData.DeleteSelection();
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

        #region Loading
        private void LoadGraphData()
        {
            LoadNodes();
        }
        
        private void LoadNodes()
        {
            foreach (var data in graphData.GetActiveNodes())
                AddNodeFromData(data);
        }

        protected void ClearNodes()
        {
            instanceNodes.AddRange(nodes.Values);

            foreach (var node in instanceNodes)
                RemoveNode(node);

            NodesCleared?.Invoke();
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
        public GraphNode GetNode(GraphNodeData nodeData)
        {
            return nodes[nodeData];
        }

        public void AddNode(GraphNode node)
        {
            nodes.Add(node.GetData(), node);

            contentViewContainer.Add(node);
        }

        public void RemoveNode(GraphNode node)
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

        public void Select(GraphElement element, bool isMultiSelect = false)
        {
            if (element == null)
                graphData.Select(null);
            else
                graphData.Select(element.GetData(), isMultiSelect);
        }
    
        private IReadOnlyList<GraphElement> GetSelection()
        {
            var selectionData = graphData.GetSelection();
            instanceElements.Clear();

            foreach (var data in selectionData)
            {
                if (nodes.TryGetValue(data, out var node))
                    instanceElements.Add(node);
            }

            return instanceElements;
        }
    }
}
