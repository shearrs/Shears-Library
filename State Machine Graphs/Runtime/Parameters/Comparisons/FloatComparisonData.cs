using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class FloatComparisonData : ParameterComparisonData<float>
    {
        public enum CompareType { LessThan, GreaterThan }

        [SerializeField] private CompareType compareType;

        public FloatComparisonData(ParameterData<float> parameter) : base(parameter)
        {
        }
    }
}
