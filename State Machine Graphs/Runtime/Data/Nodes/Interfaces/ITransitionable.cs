using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public interface ITransitionable : IGraphElementData
    {
        public IReadOnlyList<string> GetTransitionIDs();
    }
}
