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

        public override GraphElementClipboardData CopyToClipboard()
        {
            throw new NotImplementedException();
        }

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        void ILayerDefaultTarget.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerDefaultTarget.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();
    }
}
