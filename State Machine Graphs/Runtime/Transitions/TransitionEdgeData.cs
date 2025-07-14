using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class TransitionEdgeData : GraphEdgeData
    {
        public TransitionEdgeData(ITransitionable from, ITransitionable to) : base(from.ID, to.ID)
        {
        }
    }
}
