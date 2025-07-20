using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class FloatComparisonData : ParameterComparisonData<float>
    {
        public enum CompareType { LessThan, GreaterThan }

        [SerializeField] private CompareType compareType;

        public CompareType ComparisonType => compareType;

        public FloatComparisonData(ParameterData<float> parameter) : base(parameter)
        {
        }

        protected override ParameterComparison<float> CreateTypedComparison(Parameter<float> parameter) => new FloatComparison(this, (FloatParameter)parameter);
    }
}
