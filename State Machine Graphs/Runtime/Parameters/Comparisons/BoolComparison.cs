using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class BoolComparison : ParameterComparison<bool>
    {
        public BoolComparison(BoolParameter parameter, bool compareValue) : base(parameter, compareValue)
        {
        }

        public override bool EvaluateInternal() => parameter.Value == compareValue;
    }
}
