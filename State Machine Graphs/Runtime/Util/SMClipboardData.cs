using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField] private SerializableSystemType stateType;

        public SerializableSystemType StateType => stateType;

        public StateNodeClipboardData(string id, string name, Vector2 position,
            SerializableSystemType stateType) : base(id, name, position)
        {
            this.stateType = stateType;
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

        public override GraphElementData PasteDependents(Dictionary<string, GraphElementData> data)
        {
            return base.PasteDependents(data);
        }
    }

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
                    var copy = subElement.Paste(new(data.GraphData, nodeData.ID)) as GraphNodeData;

                    nodeData.AddSubNode(copy);
                }
            }

            return nodeData;
        }
    }

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