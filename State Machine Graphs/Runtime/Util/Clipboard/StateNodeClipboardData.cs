using System;
using System.Collections.Generic;
using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField]
        private SerializableType stateType;

        [SerializeField]
        private List<TransitionEdgeClipboardData> transitions;

        public SerializableType StateType => stateType;

        public StateNodeClipboardData(
            StateNodeData data,
            List<TransitionEdgeClipboardData> transitions
        )
            : base(data.ID, data.Name, data.Position)
        {
            stateType = data.StateType;
            this.transitions = transitions;
        }

        public override GraphElementData Paste(PasteData data)
        {
            var stateGraph = data.GraphData as StateMachineGraph;
            var nodeData = new StateNodeData(Name, Position, data.ParentID, StateType);

            stateGraph.AddNodeData(nodeData);

            if (data.ParentID == stateGraph.Layers[^1].ParentID)
                stateGraph.MoveNodeToCurrentLayer(nodeData);

            if (stateGraph.IsDefaultAvailable(nodeData))
                stateGraph.SetLayerDefault(nodeData);

            return nodeData;
        }

        public override void PasteDependents(PasteData data)
        {
            foreach (var transition in transitions)
                transition.Paste(data);
        }
    }
}
