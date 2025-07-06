using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public abstract class SMGraphNode : GraphNode
    {
        protected SMGraphNode(GraphNodeData data) : base(data)
        {
        }
    }
}
