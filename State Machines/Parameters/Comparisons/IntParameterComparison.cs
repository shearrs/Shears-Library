using UnityEngine;

namespace Shears.StateMachines
{
    public class IntParameterComparison : ParameterComparison<int>
    {
        public enum CompareValueType { EqualTo, LessThan, GreaterThan, LessThanOrEqualTo, GreaterThanOrEqualTo }

        [SerializeField]
        private CompareValueType compareValueType;

        public CompareValueType CompareType
        {
            get => compareValueType;
            set => compareValueType = value;
        }

        public override bool EvaluateInternal()
        {
            return compareValueType switch
            {
                CompareValueType.EqualTo => parameter.Value == compareValue,
                CompareValueType.LessThan => parameter.Value < compareValue,
                CompareValueType.GreaterThan => parameter.Value > compareValue,
                CompareValueType.LessThanOrEqualTo => parameter.Value <= compareValue,
                CompareValueType.GreaterThanOrEqualTo => parameter.Value >= compareValue,
                _ => false,
            };
        }
    }
}
