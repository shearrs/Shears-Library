using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateMachineNodeData : GraphMultiNodeData, ITransitionable, IStateNodeData
    {
        [SerializeField] private string defaultStateID;
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public string DefaultStateID => defaultStateID;

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        public void SetInitialStateID(string id) => defaultStateID = id;

        void IStateNodeData.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void IStateNodeData.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public override GraphElementClipboardData CopyToClipboard()
        {
            throw new NotImplementedException();
        }
    }
}
