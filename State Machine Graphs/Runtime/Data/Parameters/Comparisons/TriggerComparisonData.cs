using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class TriggerComparisonData : ParameterComparisonData<bool>
    {
        public TriggerComparisonData(ParameterData<bool> parameter) : base(parameter)
        {
        }

        protected override ParameterComparison<bool> CreateTypedComparison(Parameter<bool> parameter) => new TriggerComparison(this, (TriggerParameter)parameter);
    }
}
