using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class IntComparisonData : ParameterComparisonData<int>
    {
        public enum CompareType { LessThan, EqualTo, GreaterThan }

        [SerializeField] private CompareType compareType;

        public CompareType ComparisonType => compareType;

        public IntComparisonData(ParameterData<int> parameter) : base(parameter)
        {
        }

        protected override ParameterComparison<int> CreateTypedComparison(Parameter<int> parameter) => new IntComparison(this, (IntParameter)parameter);
    }
}
