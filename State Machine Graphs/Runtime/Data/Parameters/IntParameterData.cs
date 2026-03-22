using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class IntParameterData : ParameterData<int>
    {
        protected override string DefaultName => "Int Parameter";

        protected override ParameterComparisonData<int> CreateTypedComparisonData() => new IntComparisonData(this);
        public override Parameter<int> CreateTypedParameter() => new IntParameter(this);
    }
}
