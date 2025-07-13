using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class TransitionEdge : GraphEdge
    {
        private readonly TransitionEdgeData data;

        public TransitionEdge(TransitionEdgeData data, IEdgeAnchorable from, IEdgeAnchorable to) : base(data, from, to)
        {
            this.data = data;
        }
    }
}
