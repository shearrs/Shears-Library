using Shears.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class LocalParameterProvider : IParameterProvider
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string name;
        [SerializeReference] private List<Parameter> parameterDisplay = new();
#endif

        private readonly Dictionary<string, Parameter> parameters;

        public LocalParameterProvider(string name, Dictionary<string, Parameter> parameters)
        {
            this.parameters = parameters;

#if UNITY_EDITOR
            this.name = name;
            parameterDisplay.AddRange(parameters.Values);
#endif
        }

        public T GetParameter<T>(string name)
        {
            if (parameters.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    return typedParameter.Value;
                else
                    SHLogger.Log($"Parameter '{name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                SHLogger.Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);

            return default;
        }

        public void SetParameter<T>(string name, T value)
        {
            if (parameters.TryGetValue(name, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    SHLogger.Log($"Parameter '{name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                SHLogger.Log($"Parameter '{name}' not found in the state machine.", SHLogLevels.Error);
        }
    }
}
