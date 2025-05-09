using UnityEngine;

namespace InternProject.StateMachines
{
    public class BoolParameterComparison : ParameterComparison<bool>
    {
        public override bool EvaluateInternal()
        {
            return _parameter.Value == _compareValue;
        }
    }
}
