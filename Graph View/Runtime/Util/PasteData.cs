using UnityEngine;

namespace Shears.GraphViews
{
    public readonly struct PasteData
    {
        private readonly GraphData graphData;
        private readonly string parentID;

        public readonly GraphData GraphData => graphData;
        public readonly string ParentID => parentID;

        public PasteData(GraphData graphData, string parentID)
        {
            this.graphData = graphData;
            this.parentID = parentID;
        }
    }
}
