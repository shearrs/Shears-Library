using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class MultiNodeSelector : MouseManipulator
    {
        private readonly GraphView graphView;
        private GraphData graphData;

        public MultiNodeSelector(GraphView graphView)
        {
            this.graphView = graphView;

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
        }

        public void SetGraphData(GraphData graphData)
        {
            this.graphData = graphData;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(Select);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(Select);
        }

        private void Select(MouseDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                return;

            if (evt.target is not GraphMultiNode multiNode)
                return;

            graphData.OpenLayer(new(multiNode.Data));

            evt.StopImmediatePropagation();
        }
    }
}
