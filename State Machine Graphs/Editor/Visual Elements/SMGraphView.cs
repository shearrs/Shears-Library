using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMGraphView : GraphView
    {
        private StateMachineGraph graphData;

        public SMGraphView() : base()
        {
            CreateToolbar();
        }

        private void CreateToolbar()
        {
            var toolbar = new SMToolbar(graphData, SetStateMachineGraph);

            RootContainer.Insert(0, toolbar);
        }

        private void SetStateMachineGraph(StateMachineGraph graphData)
        {
            if (this.graphData == graphData)
                return;

            this.graphData = graphData;
            SetGraphData(graphData);

            var testElement = new VisualElement();
            testElement.style.backgroundColor = Color.white;
            testElement.style.width = 100;
            testElement.style.height = 100;
            testElement.style.position = Position.Absolute;

            ContentViewContainer.Add(testElement);
        }
    }
}
