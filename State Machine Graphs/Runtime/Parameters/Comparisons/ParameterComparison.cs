using Shears.Logging;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class ParameterComparison
    {
        [SerializeReference, ReadOnly] private Parameter parameter;

        public ParameterComparison(Parameter parameter)
        {
            this.parameter = parameter;
        }

        public bool Evaluate()
        {
            if (parameter == null)
            {
                SHLogger.Log("Parameter is null!", SHLogLevels.Error);
                return false;
            }

            return EvaluateInternal();
        }

        public abstract bool EvaluateInternal();
    }

    public abstract class ParameterComparison<T> : ParameterComparison
    {
        [SerializeField] protected T compareValue;
        protected readonly Parameter<T> parameter;

        protected T CompareValue => compareValue;

        public ParameterComparison(ParameterComparisonData<T> data, Parameter<T> parameter) : base(parameter)
        {
            compareValue = data.CompareValue;
            this.parameter = parameter;
        }
    }
}
