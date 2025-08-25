using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateMachineNodeClipboardData : GraphMultiNodeClipboardData
    {
        [SerializeField] private SerializableSystemType stateType;
        [SerializeField] private List<TransitionEdgeClipboardData> transitions;

        public StateMachineNodeClipboardData(StateMachineNodeData data, List<TransitionEdgeClipboardData> transitions) : base(data.ID, data.Name, data.Position)
        {
            stateType = data.StateType;
            this.transitions = transitions;
        }

        public SerializableSystemType StateType => stateType;

        public override GraphElementData Paste(PasteData data)
        {
            var stateGraph = data.GraphData as StateMachineGraph;
            var nodeData = new StateMachineNodeData(Name, Position, data.ParentID, StateType);

            stateGraph.AddNodeData(nodeData);

            if (data.ParentID == stateGraph.Layers[^1].ParentID)
                stateGraph.MoveNodeToCurrentLayer(nodeData);

            if (stateGraph.IsDefaultAvailable(nodeData))
                stateGraph.SetLayerDefault(nodeData);

            foreach (var subElement in SubElements)
            {
                if (subElement is GraphNodeClipboardData)
                {
                    var copy = subElement.Paste(new(data.GraphData, nodeData.ID, data.Mapping)) as GraphNodeData;

                    nodeData.AddSubNode(copy);
                    data.Mapping.Add(subElement.OriginalID, copy);
                }
            }

            return nodeData;
        }

        public override void PasteDependents(PasteData data)
        {
            foreach (var subElement in SubElements)
                subElement.PasteDependents(data);

            foreach (var transition in transitions)
                transition.Paste(data);
        }
    }
}