using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class ExternalStateMachineNodeData : GraphNodeData, IStateNodeData, ICopyable<ExternalStateMachineNodeClipboardData>
    {
        [SerializeField] private StateMachineGraph externalGraphData;

        public StateMachineGraph ExternalGraphData => externalGraphData;

        SerializableSystemType IStateNodeData.StateType => null;

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public ExternalStateMachineNodeData() { }

        public ExternalStateMachineNodeData(string name, Vector2 position, string parentID, StateMachineGraph externalGraphData)
        {
            this.name = name;
            this.position = position;
            this.parentID = parentID;
            this.externalGraphData = externalGraphData;
        }

        State IStateNodeData.CreateStateInstance() => new ExternalGraphState();

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        void ILayerElement.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerElement.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public ExternalStateMachineNodeClipboardData CopyToClipboard(CopyData data) => new(ID, Name, Position, externalGraphData);

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }
    }
}
