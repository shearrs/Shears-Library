using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class GraphView : VisualElement
    {
        private GraphData graphData;
        private VisualElement graphViewContainer;
        private VisualElement contentViewContainer;

        protected VisualElement GraphViewContainer => graphViewContainer;

        public VisualElement ContentViewContainer => contentViewContainer;
        public ITransform ViewTransform => contentViewContainer.transform;

        protected GraphView()
        {
            this.AddStyleSheet(GraphViewEditorUtil.GraphViewStyleSheet);
            AddToClassList("graphView");

            CreateGraphViewContainer();
            CreateContentViewContainer();
        }

        protected void SetGraphData(GraphData graphData)
        {
            this.graphData = graphData;
            focusable = true;

            graphViewContainer = new();
            contentViewContainer = new();

            CreateBackground();
            AddManipulators();

            UpdateViewTransform(graphData.Position, graphData.Scale);
        }

        private void CreateGraphViewContainer()
        {
            graphViewContainer = new()
            {
                name = "graphViewContainer"
            };

            graphViewContainer.AddToClassList("graphViewContainer");
            Add(graphViewContainer);
        }

        private void CreateContentViewContainer()
        {
            contentViewContainer = new()
            {
                name = "contentViewContainer",
                pickingMode = PickingMode.Ignore
            };

            contentViewContainer.AddToClassList("contentViewContainer");
            graphViewContainer.Add(contentViewContainer);
        }

        private void CreateBackground()
        {
            var background = new GridBackground(this);

            graphViewContainer.Insert(0, background);
        }

        private void AddManipulators()
        {
            contentViewContainer.AddManipulator(new ContentDragger());
            contentViewContainer.AddManipulator(new ContentZoomer());
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
