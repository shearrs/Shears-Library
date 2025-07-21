using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class BoolParameterData : ParameterData<bool>
    {
        protected override string DefaultName => "Bool Parameter";

        protected override ParameterComparisonData<bool> CreateTypedComparisonData() => new BoolComparisonData(this);

        public override Parameter<bool> CreateTypedParameter() => new BoolParameter(this);
    }
}
