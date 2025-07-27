using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateMachineNodeData : GraphMultiNodeData, IStateNodeData
    {
        [SerializeField] private string defaultStateID;
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public string DefaultStateID => defaultStateID;

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        public void SetInitialStateID(string id) => defaultStateID = id;

        void ILayerDefaultTarget.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerDefaultTarget.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public static StateMachineNodeData PasteFromClipboard(StateMachineNodeClipboardData data, string parentID)
        {
            var stateNode = new StateMachineNodeData
            {
                name = data.Name,
                position = data.Position,
                parentID = parentID,
                stateType = data.StateType
            };

            return stateNode;
        }

        public override GraphElementClipboardData CopyToClipboard()
        {
            return new StateMachineNodeClipboardData(Name, Position, subNodeIDs, stateType);
        }
    }
}
