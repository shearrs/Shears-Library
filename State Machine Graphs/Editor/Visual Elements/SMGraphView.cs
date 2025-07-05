using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;

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
            var toolbar = new SMToolbar(graphData, SetGraphData);

            GraphViewContainer.Insert(0, toolbar);
        }

        private void SetGraphData(StateMachineGraph graphData)
        {
            if (this.graphData == graphData)
                return;

            this.graphData = graphData;
        }
    }
}
