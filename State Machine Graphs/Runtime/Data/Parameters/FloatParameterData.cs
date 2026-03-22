using Shears.GraphViews;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class FloatParameterData : ParameterData<float>
    {
        protected override string DefaultName => "Float Parameter";

        protected override ParameterComparisonData<float> CreateTypedComparisonData() => new FloatComparisonData(this);
        public override Parameter<float> CreateTypedParameter() => new FloatParameter(this);
    }
}
