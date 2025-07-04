using UnityEngine;

namespace Shears.StateMachines
{
    public class FloatParameterComparison : ParameterComparison<float>
    {
        public enum CompareValueType { LessThan, GreaterThan }

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
                CompareValueType.LessThan => parameter.Value < compareValue,
                CompareValueType.GreaterThan => parameter.Value > compareValue,
                _ => false,
            };
        }
    }
}
