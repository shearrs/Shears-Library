using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class BoolComparisonData : ParameterComparisonData<bool>
    {
        public BoolComparisonData(ParameterData<bool> parameter) : base(parameter)
        {
        }

        protected override ParameterComparison<bool> CreateTypedComparison(Parameter<bool> parameter) => new BoolComparison(this, (BoolParameter)parameter);
    }
}
