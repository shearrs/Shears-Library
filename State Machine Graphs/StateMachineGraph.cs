using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [CreateAssetMenu(fileName = "New State Machine Graph", menuName = "Shears Library/State Machine Graph")]
    public class StateMachineGraph : GraphData
    {
        private readonly List<StateNodeData> stateNodeData = new();

        public IReadOnlyList<StateNodeData> StateNodeData => stateNodeData;

        public event Action<StateNodeData> NodeDataCreated;

        public void CreateStateNodeData(Vector2 position)
        {
            var nodeData = new StateNodeData
            {
                Position = position,
                Name = "Empty State"
            };

            stateNodeData.Add(nodeData);
            AddNodeData(nodeData);

            NodeDataCreated?.Invoke(nodeData);
        }
    }
}
