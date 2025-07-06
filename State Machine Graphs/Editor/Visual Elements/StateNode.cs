using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public class StateNode : SMGraphNode<StateNodeData>
    {
        public StateNode(StateNodeData data) : base(data)
        {
        }

        public override void Select()
        {
        }

        public override void Deselect()
        {
        }
    }
}
