using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [CreateAssetMenu(fileName = "New State Machine Graph", menuName = "Shears Library/State Machine Graph")]
    public class StateMachineGraph : GraphData
    {
        public void CreateStateNodeData(Vector2 position)
        {
            var nodeData = new StateNodeData
            {
                Position = position,
                Name = "Empty State"
            };

            AddNodeData(nodeData);
        }

        public void CreateStateMachineNodeData(Vector2 position)
        {
            var nodeData = new StateMachineNodeData
            {
                Position = position,
                Name = "New State Machine"
            };

            AddNodeData(nodeData);
        }
    }
}
