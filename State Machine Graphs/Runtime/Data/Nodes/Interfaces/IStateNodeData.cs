using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateNodeData
    {
        public string ID { get; }

        public State CreateStateInstance();
        public IReadOnlyList<string> GetTransitionIDs();
    }
}
