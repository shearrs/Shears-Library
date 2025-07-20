using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class StateNodeData : GraphNodeData, ITransitionable
    {
        [SerializeReference] private State state;
    }
}
