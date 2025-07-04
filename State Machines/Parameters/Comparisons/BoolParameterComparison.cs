using UnityEngine;

namespace Shears.StateMachines
{
    public class BoolParameterComparison : ParameterComparison<bool>
    {
        public override bool EvaluateInternal()
        {
            return parameter.Value == compareValue;
        }
    }
}
