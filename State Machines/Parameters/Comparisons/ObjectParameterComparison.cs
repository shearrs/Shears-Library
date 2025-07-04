using UnityEngine;

namespace Shears.StateMachines
{
    public class ObjectParameterComparison : ParameterComparison<Object>
    {
        public override bool EvaluateInternal()
        {
            return parameter.Value == compareValue;
        }
    }
}
