using UnityEngine;

namespace InternProject.StateMachines
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
                CompareValueType.EqualTo => _parameter.Value == _compareValue,
                CompareValueType.LessThan => _parameter.Value < _compareValue,
                CompareValueType.GreaterThan => _parameter.Value > _compareValue,
                CompareValueType.LessThanOrEqualTo => _parameter.Value <= _compareValue,
                CompareValueType.GreaterThanOrEqualTo => _parameter.Value >= _compareValue,
                _ => false,
            };
        }
    }
}
