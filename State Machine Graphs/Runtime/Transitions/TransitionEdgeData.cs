using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class TransitionEdgeData : GraphEdgeData
    {
        public TransitionEdgeData(ITransitionable from, ITransitionable to) : base(from.ID, to.ID)
        {
        }
    }
}
