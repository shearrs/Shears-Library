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

        public StateNodeClipboardData(string name, Vector2 position, List<string> edges, SerializableSystemType stateType) : base(name, position, edges)
        {
            this.stateType = stateType;
        }
    }
}