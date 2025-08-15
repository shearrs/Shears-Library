using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeData : GraphNodeData, IStateNodeData, ICopyable<StateNodeClipboardData>
    {
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public StateNodeData() { }

        public StateNodeData(string name, Vector2 position, string parentID, SerializableSystemType stateType)
        {
            this.name = name;
            this.position = position;
            this.parentID = parentID;
            this.stateType = stateType;
        }

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        State IStateNodeData.CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        void ILayerElement.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerElement.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public static StateNodeData PasteFromClipboard(StateNodeClipboardData data, string parentID)
        {
            var stateNode = new StateNodeData
            {
                name = data.Name,
                position = data.Position,
                parentID = parentID,
                stateType = data.StateType
            };

            return stateNode;
        }

        public StateNodeClipboardData CopyToClipboard(CopyData data) => new(Name, Position, stateType);

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }
    }
}
