using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class TransitionEdge : GraphEdge
    {
        private readonly TransitionEdgeData data;

        public TransitionEdge(TransitionEdgeData data, GraphElement from, GraphElement to) : base(data, from, to)
        {
            this.data = data;
        }
    }
}
