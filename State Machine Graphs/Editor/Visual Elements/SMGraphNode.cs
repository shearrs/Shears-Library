using Shears.GraphViews;
using Shears.GraphViews.Editor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public abstract class SMGraphNode<T> : GraphNode<T> where T : GraphNodeData
    {
        public SMGraphNode(T data) : base(data)
        {
        }
    }
}
