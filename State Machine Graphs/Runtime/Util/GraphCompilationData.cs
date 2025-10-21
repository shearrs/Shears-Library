using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class ParameterDictionary : SerializableReferenceDictionary<string, Parameter>
    {
    }

    [Serializable]
    public class StateDictionary : SerializableReferenceDictionary<string, State>
    {
    }

    public class GraphCompilationData : ScriptableObject
    {
        [SerializeField] private ParameterDictionary parameterNames;
        [SerializeField] private ParameterDictionary parameterIDs;
        [SerializeField] private StateDictionary stateIDs;
        [SerializeField] private List<LocalParameterProvider> parameterProviders;
        [SerializeReference] private State defaultState;

        public Dictionary<string, Parameter> ParameterNames => parameterNames;
        public Dictionary<string, Parameter> ParameterIDs => parameterIDs;
        public Dictionary<string, State> StateIDs => stateIDs;
        public List<LocalParameterProvider> ParameterProviders => parameterProviders;
        public State DefaultState => defaultState;

        public static GraphCompilationData Create(ParameterDictionary parameterNames, ParameterDictionary parameterIDs, StateDictionary stateIDs, List<LocalParameterProvider> parameterProviders, State defaultState)
        {
            var data = CreateInstance<GraphCompilationData>();

            data.name = "Compilation Data";
            data.parameterNames = parameterNames;
            data.parameterIDs = parameterIDs;
            data.stateIDs = stateIDs;
            data.parameterProviders = parameterProviders;
            data.defaultState = defaultState;

            return data;
        }

        public GraphCompilationData(ParameterDictionary parameterNames, ParameterDictionary parameterIDs, StateDictionary stateIDs, List<LocalParameterProvider> parameterProviders, State defaultState)
        {
            this.parameterNames = parameterNames;
            this.parameterIDs = parameterIDs;
            this.stateIDs = stateIDs;
            this.parameterProviders = parameterProviders;
            this.defaultState = defaultState;
        }
    }
}
