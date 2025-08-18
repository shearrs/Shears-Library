using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public readonly struct PasteData
    {
        private readonly GraphData graphData;
        private readonly string parentID;
        private readonly Dictionary<string, GraphElementData> mapping;

        public readonly GraphData GraphData => graphData;
        public readonly string ParentID => parentID;
        public readonly Dictionary<string, GraphElementData> Mapping => mapping;

        public PasteData(GraphData graphData, string parentID, Dictionary<string, GraphElementData> mapping)
        {
            this.graphData = graphData;
            this.parentID = parentID;
            this.mapping = mapping;
        }
    }
}
