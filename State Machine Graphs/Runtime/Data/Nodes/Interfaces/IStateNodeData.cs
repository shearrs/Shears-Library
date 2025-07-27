using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateNodeData : ITransitionable, ILayerDefaultTarget
    {
        public string Name { get; }

        public State CreateStateInstance();
    }
}
