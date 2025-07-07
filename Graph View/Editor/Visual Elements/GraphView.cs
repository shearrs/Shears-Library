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
        private GraphData graphData;
        private VisualElement rootContainer;
        private VisualElement graphViewContainer;
        private VisualElement contentViewContainer;
        private GridBackground gridBackground;

        protected VisualElement RootContainer => rootContainer;
        public VisualElement GraphViewContainer => graphViewContainer;
        public VisualElement ContentViewContainer => contentViewContainer;
        public ITransform ViewTransform => contentViewContainer.transform;
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

        #region Initialization
        protected void SetGraphData(GraphData graphData)
        {
            if (graphData == this.graphData || graphData == null)
                return;

            this.graphData = graphData;
            multiNodeSelector.SetGraphData(graphData);
            nodeDragger.SetGraphData(graphData);

            graphData.LayersChanged -= ReloadLayer;
            graphData.LayersChanged += ReloadLayer;

            CreateBackground();
            AddManipulators();
            UpdateViewTransform(graphData.Position, graphData.Scale);

            GraphEditorState.instance.SetGraphData(graphData);
            OnGraphDataSet(graphData);

            LoadGraphData();
        }

        protected void ClearGraphData()
        {
            if (graphData == null) 
                return; 

            graphData.LayersChanged -= ReloadLayer;
            graphData = null;

            contentViewContainer.Clear();
            graphViewContainer.Remove(gridBackground);
            graphViewContainer.RemoveManipulator(contentDragger);
            graphViewContainer.RemoveManipulator(contentZoomer);
            graphViewContainer.RemoveManipulator(contentSelector);
            graphViewContainer.RemoveManipulator(multiNodeSelector);
            graphViewContainer.RemoveManipulator(nodeDragger);

            GraphEditorState.instance.SetGraphData(null);
            OnGraphDataCleared();
        }

        protected void ReloadLayer()
        {
            ClearNodes();
            LoadNodes(graphData.GetActiveNodes());
        }

        protected virtual void OnGraphDataSet(GraphData graphData)
        {
        }

        protected virtual void OnGraphDataCleared()
        {
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

        #region Loading
        private void LoadGraphData()
        {
            LoadNodes(graphData.GetActiveNodes());
        }

        protected abstract void LoadNodes(IReadOnlyCollection<GraphNodeData> nodeData);
        protected abstract void ClearNodes();
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
        #endregion

        public void Select(GraphElement element, bool isMultiSelect = false)
        {
            if (element == null)
                graphData.Select(null);
            else
                graphData.Select(element.GetData(), isMultiSelect);
        }
    }
}
