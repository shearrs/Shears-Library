using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class IntComparisonData : ParameterComparisonData<int>
    {
        public enum CompareType { LessThan, EqualTo, GreaterThan }

        [SerializeField] private CompareType compareType;

        public IntComparisonData(ParameterData<int> parameter) : base(parameter)
        {
        }
    }
}
