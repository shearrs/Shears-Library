using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class TriggerComparison : ParameterComparison<bool>
    {
        public TriggerComparison(TriggerParameter parameter, bool compareValue) : base(parameter, compareValue)
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
