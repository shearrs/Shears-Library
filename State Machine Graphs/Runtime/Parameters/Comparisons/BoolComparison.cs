using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class BoolComparison : ParameterComparison<bool>
    {
        public BoolComparison(BoolComparisonData data, BoolParameter parameter) : base(data, parameter)
        {
        }

        public override bool EvaluateInternal()
        {
            return parameter.Value == CompareValue;
        }
    }
}
