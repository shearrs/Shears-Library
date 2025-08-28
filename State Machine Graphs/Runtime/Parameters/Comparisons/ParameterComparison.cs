using Shears.Logging;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class ParameterComparison
    {
        [SerializeReference, ReadOnly] private Parameter untypedParameter;

        public ParameterComparison(Parameter parameter)
        {
            untypedParameter = parameter;
        }

        public bool Evaluate()
        {
            if (untypedParameter == null)
            {
                SHLogger.Log("Parameter is null!", SHLogLevels.Error);
                return false;
            }

            return EvaluateInternal();
        }

        public abstract bool EvaluateInternal();
    }

    [System.Serializable]
    public abstract class ParameterComparison<T> : ParameterComparison
    {
        [SerializeField] protected T compareValue;
        [SerializeReference, ReadOnly] protected Parameter<T> parameter;

        protected T CompareValue => compareValue;

        public ParameterComparison(ParameterComparisonData<T> data, Parameter<T> parameter) : base(parameter)
        {
            compareValue = data.CompareValue;
            this.parameter = parameter;
        }
    }
}
