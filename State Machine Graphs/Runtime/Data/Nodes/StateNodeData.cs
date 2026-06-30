using System;
using System.Collections.Generic;
using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeData : GraphNodeData, IStateNodeData, ICopyable<StateNodeClipboardData>
    {
        [SerializeField]
        private SerializableType stateType;

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public SerializableType StateType
        {
            get => stateType;
            set => stateType = value;
        }

        public event Action StateTypeChanged;

        public StateNodeData(SerializableType stateType)
        {
            this.stateType = stateType;
        }

        public StateNodeData(
            string name,
            Vector2 position,
            string parentID,
            SerializableType stateType
        )
        {
            this.name = name;
            this.position = position;
            this.parentID = parentID;
            this.stateType = stateType;
        }

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        State IStateNodeData.CreateStateInstance() =>
            (State)Activator.CreateInstance(stateType.SystemType);

        void ILayerElement.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerElement.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public StateNodeClipboardData CopyToClipboard(CopyData data)
        {
            var transitions = new List<TransitionEdgeClipboardData>();

            foreach (var edgeID in Edges)
            {
                if (!data.GraphData.TryGetData(edgeID, out TransitionEdgeData transition))
                    continue;

                transitions.Add(new(transition));
            }

            return new(this, transitions);
        }

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }

        public void SignalStateChanged() => StateTypeChanged?.Invoke();
    }
}
