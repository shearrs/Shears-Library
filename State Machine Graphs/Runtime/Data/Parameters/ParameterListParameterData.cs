using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class ParameterListParameterData : ParameterData<ParameterList>
    {
        protected override string DefaultName => "Parameter List";

        public override Parameter<ParameterList> CreateTypedParameter()
        {
            Debug.LogError("Parameter Lists can not be made concrete!");
            throw new System.NotImplementedException();
        }

        protected override ParameterComparisonData<ParameterList> CreateTypedComparisonData()
        {
            Debug.LogError("Parameter Lists can not be compared!");
            throw new System.NotImplementedException();
        }
    }
}
