using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class FloatParameterData : ParameterData<float>
    {
        protected override string DefaultName => "Float Parameter";

        protected override ParameterComparisonData<float> CreateTypedComparisonData() => new FloatComparisonData(this);
        protected override Parameter<float> CreateTypedParameter() => new FloatParameter(this);
    }
}
