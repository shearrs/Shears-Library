using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeData : GraphNodeData, ITransitionable, IStateNodeData
    {
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        void IStateNodeData.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void IStateNodeData.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();
    }
}
