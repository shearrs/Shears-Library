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

        public StateNodeClipboardData(string name, Vector2 position, SerializableSystemType stateType) : base(name, position)
        {
            this.stateType = stateType;
        }
    }

    [Serializable]
    public class StateMachineNodeClipboardData : GraphMultiNodeClipboardData
    {
        [SerializeField] private SerializableSystemType stateType;

        public StateMachineNodeClipboardData(string name, Vector2 position, List<string> subNodeIDs, 
            SerializableSystemType stateType) : base(name, position, subNodeIDs)
        {
            this.stateType = stateType;
        }

        public SerializableSystemType StateType => stateType;
    }

    [Serializable]
    public class ExternalStateMachineNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField] private StateMachineGraph externalGraphData;

        public StateMachineGraph ExternalGraphData => externalGraphData;

        public ExternalStateMachineNodeClipboardData(string name, Vector2 position, StateMachineGraph externalGraphData) 
            : base(name, position)
        {
            this.externalGraphData = externalGraphData;
        }
    }
}