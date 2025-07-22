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

        public StateMachineNodeData()
        {
            SubNodeAdded += OnSubNodeAdded;
            SubNodeRemoved += OnSubNodeRemoved;
        }

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        public void SetInitialStateID(string id) => defaultStateID = id;

        private void OnSubNodeAdded(string id)
        {
            if (SubNodeIDs.Count == 1)
                defaultStateID = id;
        }

        private void OnSubNodeRemoved(string id)
        {
            //if (id != defaultStateID)
            //    return;

            //if (SubNodeIDs.Count > 0)
            //    defaultStateID = SubNodeIDs[0];
        }

        void IStateNodeData.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void IStateNodeData.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();
    }
}
