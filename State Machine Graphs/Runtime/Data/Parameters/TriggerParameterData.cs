using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class TriggerParameterData : ParameterData<bool>
    {
        protected override string DefaultName => "Trigger Parameter";

        protected override ParameterComparisonData<bool> CreateTypedComparisonData() => new TriggerComparisonData(this);
        protected override Parameter<bool> CreateTypedParameter() => new TriggerParameter(this);
    }
}
