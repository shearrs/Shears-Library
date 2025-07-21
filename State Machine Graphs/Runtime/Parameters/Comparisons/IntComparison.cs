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
                IntComparisonData.CompareType.LessThan => parameter.Value < CompareValue,
                IntComparisonData.CompareType.EqualTo => parameter.Value == CompareValue,
                IntComparisonData.CompareType.GreaterThan => parameter.Value > CompareValue,
                _ => false,
            };
        }
    }
}
