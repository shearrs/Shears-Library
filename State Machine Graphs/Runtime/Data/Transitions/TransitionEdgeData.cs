using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class TransitionEdgeData : GraphEdgeData
    {
        [SerializeReference] private List<ParameterComparisonData> comparisonData = new();

        public IReadOnlyList<ParameterComparisonData> ComparisonData => comparisonData;

        public TransitionEdgeData(ITransitionable from, ITransitionable to) : base(from.ID, to.ID)
        {
        }

        public void AddComparisonData(ParameterComparisonData data)
        {
            comparisonData.Add(data);
        }
    }
}
