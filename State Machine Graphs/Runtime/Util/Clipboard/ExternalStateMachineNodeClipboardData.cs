using Shears.GraphViews;
using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class ExternalStateMachineNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField] private StateMachineGraph externalGraphData;

        public StateMachineGraph ExternalGraphData => externalGraphData;

        public ExternalStateMachineNodeClipboardData(string id, string name, Vector2 position,
            StateMachineGraph externalGraphData) 
            : base(id, name, position)
        {
            this.externalGraphData = externalGraphData;
        }

        public override GraphElementData Paste(PasteData data)
        {
            var stateGraph = data.GraphData as StateMachineGraph;
            var nodeData = new ExternalStateMachineNodeData(Name, Position, data.ParentID, ExternalGraphData);

            stateGraph.AddNodeData(nodeData);

            if (data.ParentID == stateGraph.Layers[^1].ParentID)
                stateGraph.MoveNodeToCurrentLayer(nodeData);

            if (stateGraph.IsDefaultAvailable(nodeData))
                stateGraph.SetLayerDefault(nodeData);

            return nodeData;
        }
    }
}