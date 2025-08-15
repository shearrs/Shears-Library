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

        public static ExternalStateMachineNodeData PasteFromClipboard(ExternalStateMachineNodeClipboardData data, string parentID)
        {
            var node = new ExternalStateMachineNodeData
            {
                name = data.Name,
                position = data.Position,
                parentID = parentID,
                externalGraphData = data.ExternalGraphData
            };

            return node;
        }

        public ExternalStateMachineNodeClipboardData CopyToClipboard(CopyData data) => new(Name, Position, externalGraphData);

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }
    }
}
