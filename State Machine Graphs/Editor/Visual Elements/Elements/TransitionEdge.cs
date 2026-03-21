using Shears.GraphViews.Editor;

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
