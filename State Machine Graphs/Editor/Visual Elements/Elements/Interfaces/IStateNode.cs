using Shears.GraphViews.Editor;
using UnityEngine;

namespace Shears.StateMachineGraphs.Editor
{
    public interface IStateNode : IEdgeAnchorable
    {
        public IStateNodeData Data { get; }
    }
}
