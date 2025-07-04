using UnityEngine;

namespace Shears.StateMachines
{
    public class TriggerParameterComparison : ParameterComparison<Trigger>
    {
        public override bool EvaluateInternal()
        {
            bool result = parameter.Value.Value == Trigger.Active.Value;

            parameter.Value = Trigger.Inactive;

            return result;
        }
    }
}
