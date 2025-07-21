using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateNodeData
    {
        public string Name { get; }
        public string ID { get; }
        public string ParentID { get; }

        public State CreateStateInstance();
        public IReadOnlyList<string> GetTransitionIDs();
        public void OnSetAsLayerDefault();
        public void OnRemoveLayerDefault();
    }
}
