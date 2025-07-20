using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class TriggerComparison : ParameterComparison<bool>
    {
        public TriggerComparison(TriggerComparisonData data, TriggerParameter parameter) : base(data, parameter)
        {
        }

        public override bool EvaluateInternal()
        {
            bool value = parameter.Value;

            parameter.Value = false;

            return value;
        }
    }
}
