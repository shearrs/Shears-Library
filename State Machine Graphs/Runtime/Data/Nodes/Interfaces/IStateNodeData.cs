using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface IStateNodeData : ITransitionable, ILayerElement
    {
        public string Name { get; }

        public State CreateStateInstance();
    }
}
