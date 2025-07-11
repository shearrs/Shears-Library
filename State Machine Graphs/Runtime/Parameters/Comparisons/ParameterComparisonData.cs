using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public abstract class ParameterComparisonData
    {
        public abstract ParameterData Parameter { get; }
    }

    [System.Serializable]
    public abstract class ParameterComparisonData<T> : ParameterComparisonData
    {
        [field: SerializeReference] private ParameterData<T> typedParameter;
        [SerializeField] private T compareValue;

        public override ParameterData Parameter => typedParameter;
        public ParameterData<T> TypedParameter => typedParameter;

        public ParameterComparisonData(ParameterData<T> parameter)
        {
            typedParameter = parameter;
        }
    }
}
