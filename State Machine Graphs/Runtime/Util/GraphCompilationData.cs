using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class ParameterDictionary : SerializableDictionary<string, Parameter>
    {
    }

    [Serializable]
    public class StateDictionary : SerializableReferenceDictionary<string, IState>
    {
    }

    [Serializable]
    public struct GraphCompilationData
    {
        [SerializeField] private ParameterDictionary parameterNames;
        [SerializeField] private ParameterDictionary parameterIDs;
        [SerializeField] private StateDictionary stateIDs;
        [SerializeField] private List<LocalParameterProvider> parameterProviders;
        [SerializeReference] private IState defaultState;

        public readonly Dictionary<string, Parameter> ParameterNames => parameterNames;
        public readonly Dictionary<string, Parameter> ParameterIDs => parameterIDs;
        public readonly Dictionary<string, IState> StateIDs => stateIDs;
        public readonly List<LocalParameterProvider> ParameterProviders => parameterProviders;
        public readonly IState DefaultState => defaultState;

        public GraphCompilationData(ParameterDictionary parameterNames, ParameterDictionary parameterIDs, StateDictionary stateIDs, List<LocalParameterProvider> parameterProviders, IState defaultState)
        {
            this.parameterNames = parameterNames;
            this.parameterIDs = parameterIDs;
            this.stateIDs = stateIDs;
            this.parameterProviders = parameterProviders;
            this.defaultState = defaultState;
        }
    }
}
