using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class FloatParameterData : ParameterData<float>
    {
        protected override string DefaultName => "Float Parameter";

        protected override ParameterComparisonData<float> CreateTypedComparison() => new FloatComparisonData(this);
    }
}
