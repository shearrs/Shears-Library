using UnityEngine;

namespace Shears.StateMachines
{
    public class IntParameterComparison : ParameterComparison<int>
    {
        public enum CompareValueType { EqualTo, LessThan, GreaterThan, LessThanOrEqualTo, GreaterThanOrEqualTo }

        [SerializeField]
        private CompareValueType _compareValueType;

        public CompareValueType CompareType
        {
            get => _compareValueType;
            set => _compareValueType = value;
        }

        public override bool EvaluateInternal()
        {
            return _compareValueType switch
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
