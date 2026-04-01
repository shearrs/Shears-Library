using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class TransitionData
    {
        [SerializeReference]
        private List<ParameterComparisonData> comparisonData = new();

        public IReadOnlyList<ParameterComparisonData> ComparisonData => comparisonData;

        public TransitionData()
        {
            comparisonData = new();
        }

        public void AddComparisonData(ParameterComparisonData data)
        {
            comparisonData.Add(data);
        }
    }
}
