using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class FloatComparison : ParameterComparison<float>
    {
        [SerializeField] private FloatComparisonData.CompareType compareType;

        public FloatComparison(FloatComparisonData data, FloatParameter parameter) : base(data, parameter)
        {
            compareType = data.ComparisonType;
        }

        public override bool EvaluateInternal()
        {
            return compareType switch
            {
                FloatComparisonData.CompareType.LessThan => parameter.Value < CompareValue,
                FloatComparisonData.CompareType.GreaterThan => parameter.Value > CompareValue,
                _ => false
            };
        }
    }
}
