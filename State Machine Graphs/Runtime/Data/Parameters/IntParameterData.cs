using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class IntParameterData : ParameterData<int>
    {
        protected override string DefaultName => "Int Parameter";

        protected override ParameterComparisonData<int> CreateTypedComparisonData() => new IntComparisonData(this);
        protected override Parameter<int> CreateTypedParameter() => new IntParameter(this);
    }
}
