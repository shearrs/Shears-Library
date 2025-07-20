using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateNodeData
    {
        public State CreateStateInstance();
    }
}
