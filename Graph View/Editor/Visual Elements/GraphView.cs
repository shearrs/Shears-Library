using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class GraphView : VisualElement
    {
        private readonly ContentDragger contentDragger;
        private readonly ContentZoomer contentZoomer;
        private GraphData graphData;
        private VisualElement rootContainer;
        private VisualElement graphViewContainer;
        private VisualElement contentViewContainer;
        private GridBackground gridBackground;

        protected VisualElement RootContainer => rootContainer;
        public VisualElement GraphViewContainer => graphViewContainer;
        public VisualElement ContentViewContainer => contentViewContainer;
        public ITransform ViewTransform => contentViewContainer.transform;

        protected GraphView()
        {
            this.AddStyleSheet(GraphViewEditorUtil.GraphViewStyleSheet);
            AddToClassList(GraphViewEditorUtil.GraphViewClassName);

            contentDragger = new(this);
            contentZoomer = new(this);

            CreateRootContainer();
            CreateGraphViewContainer();
            CreateContentViewContainer();
        }

        protected void SetGraphData(GraphData graphData)
        {
            if (graphData == null)
            {
                graphViewContainer.Remove(gridBackground);
                contentViewContainer.Clear();
                contentViewContainer.RemoveManipulator(contentDragger);
                contentViewContainer.RemoveManipulator(contentZoomer);

                return;
            }

            this.graphData = graphData;
            focusable = true;

            CreateBackground();
            AddManipulators();

            UpdateViewTransform(graphData.Position, graphData.Scale);
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
        }

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
    }
}
