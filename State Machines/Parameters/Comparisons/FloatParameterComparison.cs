using UnityEngine;

namespace Shears.StateMachines
{
    public class FloatParameterComparison : ParameterComparison<float>
    {
        public enum CompareValueType { LessThan, GreaterThan }

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
                CompareValueType.LessThan => _parameter.Value < _compareValue,
                CompareValueType.GreaterThan => _parameter.Value > _compareValue,
                _ => false,
            };
        }
    }
}
