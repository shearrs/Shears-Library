using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class ParameterComparisonData
    {
        [SerializeField] private string parameterID;

        public string ParameterID => parameterID;

        public ParameterComparisonData()
        {
        }

        public ParameterComparisonData(ParameterData parameter)
        {
            parameterID = parameter.ID;
        }
    }

    [System.Serializable]
    public abstract class ParameterComparisonData<T> : ParameterComparisonData
    {
        [SerializeField] private T compareValue;

        public ParameterComparisonData(ParameterData<T> parameter) : base(parameter)
        {
        }
    }
}
