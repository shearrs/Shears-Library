using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class ExternalStateMachineNodeData : GraphNodeData, IStateNodeData
    {
        [SerializeField] private StateMachineGraph externalGraphData;

        public StateMachineGraph ExternalGraphData => externalGraphData;

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        State IStateNodeData.CreateStateInstance() => new ExternalGraphState();

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        void ILayerDefaultTarget.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerDefaultTarget.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public override GraphElementClipboardData CopyToClipboard() => new ExternalStateMachineNodeClipboardData(Name, Position, externalGraphData);

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
    }
}
