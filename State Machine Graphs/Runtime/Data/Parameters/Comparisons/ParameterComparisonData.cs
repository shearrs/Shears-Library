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

        public abstract ParameterComparison CreateComparison(Parameter parameter);
    }

    [System.Serializable]
    public abstract class ParameterComparisonData<T> : ParameterComparisonData
    {
        [SerializeField] private T compareValue;

        public T CompareValue => compareValue;

        public ParameterComparisonData(ParameterData<T> parameter) : base(parameter)
        {
        }

        public override ParameterComparison CreateComparison(Parameter parameter)
        {
            if (parameter is not Parameter<T> typedParameter)
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
