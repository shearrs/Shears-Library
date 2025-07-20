using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public abstract class ParameterComparison
    {
        private readonly Parameter parameter;

        public ParameterComparison(Parameter parameter)
        {
            this.parameter = parameter;
        }

        public bool Evaluate()
        {
            if (parameter == null)
                return false;

            return EvaluateInternal();
        }

        public abstract bool EvaluateInternal();
    }

    public abstract class ParameterComparison<T> : ParameterComparison
    {
        protected readonly Parameter<T> parameter;
        protected readonly T compareValue;

        public ParameterComparison(ParameterComparisonData<T> data, Parameter<T> parameter) : base(parameter)
        {
            this.compareValue = data.CompareValue;
            this.parameter = parameter;
        }
    }
}
