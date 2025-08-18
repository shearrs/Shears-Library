using Shears.GraphViews;
using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateMachineNodeClipboardData : GraphMultiNodeClipboardData
    {
        [SerializeField] private SerializableSystemType stateType;

        public StateMachineNodeClipboardData(string id, string name, Vector2 position,
            SerializableSystemType stateType) : base(id, name, position)
        {
            this.stateType = stateType;
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
                }
            }

            return nodeData;
        }
    }
}