using Shears.GraphViews;
using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeData : GraphNodeData, ITransitionable, IStateNodeData
    {
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);
    }
}
