using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class LocalParameterProvider : IParameterProvider
    {
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string name;
        [SerializeReference] private List<Parameter> parameterDisplay = new();
#endif

        private readonly Dictionary<string, SMID> parameterNameCache = new();
        private readonly Dictionary<SMID, Parameter> parameters = new();

        public LocalParameterProvider(string name, Dictionary<string, Parameter> parameters)
        {
            foreach (var parameterName in parameters.Keys)
            {
                var id = SMID.Create();
                parameterNameCache.Add(parameterName, id);
                this.parameters.Add(id, parameters[parameterName]);
            }

#if UNITY_EDITOR
            this.name = name;
            parameterDisplay.AddRange(parameters.Values);
#endif
        }

        public T GetParameter<T>(string name) => GetParameter<T>(GetParameterID(name));
        public T GetParameter<T>(SMID id)
        {
            if (parameters.TryGetValue(id, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    return typedParameter.Value;
                else
                    SHLogger.Log($"Parameter '{parameter.Name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                SHLogger.Log($"Could not find parameter with id '{id}' in the state machine.", SHLogLevels.Error);

            return default;
        }

        public void SetParameter<T>(string name, T value) => SetParameter(GetParameterID(name), value);
        public void SetParameter<T>(SMID id, T value)
        {
            if (parameters.TryGetValue(id, out var parameter))
            {
                if (parameter is Parameter<T> typedParameter)
                    typedParameter.Value = value;
                else
                    SHLogger.Log($"Parameter '{parameter.Name}' is not of type {typeof(T)}.", SHLogLevels.Error);
            }
            else
                SHLogger.Log($"Could not find parameter with id '{id}' in the state machine.", SHLogLevels.Error);
        }

        public SMID GetParameterID(string name)
        {
            if (parameterNameCache.TryGetValue(name, out var id))
                return id;
            else
            {
                SHLogger.Log($"Could not find parameter with name '{name}'.", SHLogLevels.Error);
                return SMID.Empty;
            }
        }
    }
}
