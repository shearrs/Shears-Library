using Shears.Logging;
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

        public abstract ParameterComparison CreateComparison(StateMachineGraph graphData);
    }

    [System.Serializable]
    public abstract class ParameterComparisonData<T> : ParameterComparisonData
    {
        [SerializeField] private T compareValue;

        public T CompareValue => compareValue;

        public ParameterComparisonData(ParameterData<T> parameter) : base(parameter)
        {
        }

        public override ParameterComparison CreateComparison(StateMachineGraph graphData)
        {
            if (!graphData.TryGetData<ParameterData>(ParameterID, out var parameter))
            {
                SHLogger.Log("Could not find parameter with ID: " + ParameterID, SHLogLevels.Error);
                return null;
            }
            else if (parameter is not Parameter<T> typedParameter)
            {
                SHLogger.Log($"Parameter {parameter.Name} is not of type {typeof(T).Name}!", SHLogLevels.Error);
                return null;
            }
            else
                return CreateTypedComparison(typedParameter);
        }
        protected abstract ParameterComparison<T> CreateTypedComparison(Parameter<T> parameter);
    }
}
