using UnityEngine;

namespace InternProject.StateMachines
{
    public class TriggerParameterComparison : ParameterComparison<Trigger>
    {
        public override bool EvaluateInternal()
        {
            bool result = _parameter.Value.Value == Trigger.Active.Value;

            _parameter.Value = Trigger.Inactive;

            return result;
        }
    }
}
