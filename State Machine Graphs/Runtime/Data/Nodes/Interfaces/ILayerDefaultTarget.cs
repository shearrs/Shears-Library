using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface ILayerDefaultTarget : IGraphElementData
    {
        public string ParentID { get; }

        public void OnSetAsLayerDefault();
        public void OnRemoveLayerDefault();
    }
}
