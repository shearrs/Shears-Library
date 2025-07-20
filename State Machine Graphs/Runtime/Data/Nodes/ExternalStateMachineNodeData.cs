using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class ExternalStateMachineNodeData : GraphNodeData, ITransitionable
    {
        [SerializeField] private StateMachineGraph externalGraphData;
    }
}
