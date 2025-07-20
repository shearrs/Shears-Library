using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class IntComparison : ParameterComparison<int>
    {
        private readonly IntComparisonData.CompareType compareType;

        public IntComparison(IntComparisonData data, IntParameter parameter) : base(data, parameter)
        {
            compareType = data.ComparisonType;
        }

        public override bool EvaluateInternal()
        {
            return compareType switch
            {
                IntComparisonData.CompareType.LessThan => parameter.Value < compareValue,
                IntComparisonData.CompareType.EqualTo => parameter.Value == compareValue,
                IntComparisonData.CompareType.GreaterThan => parameter.Value > compareValue,
                _ => false,
            };
        }
    }
}
