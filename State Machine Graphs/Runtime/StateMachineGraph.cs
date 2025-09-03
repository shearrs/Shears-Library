using Shears.GraphViews;
using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [CreateAssetMenu(fileName = "New State Machine Graph", menuName = "Shears Library/State Machine Graph")]
    public class StateMachineGraph : GraphData
    {
        [Header("State Machine Elements")]
        [SerializeField] private string rootDefaultStateID;
        [SerializeField] private List<string> parameters = new();
        [SerializeField] private GraphCompilationData compilationData;
        private readonly List<IStateNodeData> instanceStateNodes = new();
        private readonly List<ParameterData> instanceParameters = new();

        public string RootDefaultStateID => rootDefaultStateID;
        public GraphCompilationData CompilationData => compilationData;

        public event Action<ParameterData> ParameterDataAdded;
        public event Action<ParameterData> ParameterDataRemoved;

        protected override void OnDeleteSelection(IReadOnlyList<GraphElementData> selection)
        {
            foreach (var element in selection)
            {
                if (element is IStateNodeData stateNodeData && IsLayerDefault(stateNodeData))
                {
                    var activeNodes = GetActiveNodes();

                    if (activeNodes.Count == 0)
                    {
                        ClearLayerDefault(GetLayer(stateNodeData));
                        return;
                    }

                    foreach (var node in activeNodes)
                    {
                        if (node is IStateNodeData activeStateNode)
                        {
                            SetLayerDefault(activeStateNode);

                            break;
                        }
                    }
                }
                else if (element is ParameterData parameterData)
                    RemoveParameter(parameterData);
            }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(rootDefaultStateID) && rootNodes.Count > 0)
                rootDefaultStateID = rootNodes[0];
        }

        #region Compilation
        public void Compile()
        {
            var parameterNames = new ParameterDictionary();
            var parameterIDs = new ParameterDictionary();
            var stateIDs = new StateDictionary();
            var parameterProviders = new List<LocalParameterProvider>();
            State defaultState;

            foreach (var parameterData in GetParameters())
            {
                var parameter = CreateParameter(parameterData);

                parameterNames.Add(parameter.Name, parameter);
                parameterIDs.Add(parameterData.ID, parameter);
            }

            var stateNodes = GetStateNodes();

            foreach (var stateNode in stateNodes)
            {
                var state = CreateState(stateNode);

                stateIDs.Add(stateNode.ID, state);

                // TODO: prevent cyclic dependencies
                if (stateNode is ExternalStateMachineNodeData externalNode)
                {
                    var graphData = externalNode.ExternalGraphData;

                    if (graphData == null)
                        continue;

                    var compileData = graphData.CompilationData;
                    var parameterProvider = new LocalParameterProvider(state.Name, compileData.ParameterNames);
                    parameterProviders.Add(parameterProvider);

                    state.ParameterProvider = parameterProvider;

                    // this mode of unique key can still lead to duplicates
                    // we should make it include the full path of the node
                    foreach (var stateID in compileData.StateIDs)
                    {
                        var subState = compileData.StateIDs[stateID.Key];
                        var key = $"{graphData.ID}:{stateID.Key}";

                        subState.ParameterProvider = parameterProvider;

                        stateIDs.Add(key, subState);
                        subState.ParentState ??= state;
                    }

                    state.DefaultSubState = compileData.DefaultState;
                }
            }

            foreach (var stateNode in stateNodes)
            {
                if (GraphLayer.IsRootID(stateNode.ParentID))
                    continue;

                var state = stateIDs[stateNode.ID];
                var parent = stateIDs[stateNode.ParentID];

                state.ParentState = parent;

                if (IsLayerDefault(stateNode))
                    parent.DefaultSubState = state;
            }

            foreach (var stateNode in stateNodes)
                CreateTransitions(stateNode, stateIDs[stateNode.ID], stateIDs, parameterIDs);

            defaultState = stateIDs[RootDefaultStateID];

            compilationData = new GraphCompilationData(parameterNames, parameterIDs, stateIDs, parameterProviders, defaultState);
        }

        private Parameter CreateParameter(ParameterData data) => data.CreateParameter();

        private State CreateState(IStateNodeData data)
        {
            var state = data.CreateStateInstance();
            state.Name = data.Name;

            return state;
        }

        private void CreateTransitions(IStateNodeData data, State state, Dictionary<string, State> states, Dictionary<string, Parameter> parameterIDs)
        {
            if (state.TransitionCount > 0) // already initialized
                return;

            var transitionIDs = data.GetTransitionIDs();

            foreach (var id in transitionIDs)
            {
                if (!TryGetData(id, out TransitionEdgeData transitionData))
                {
                    SHLogger.Log("Could not find transition with id: " + id, SHLogLevels.Error);
                    continue;
                }

                if (transitionData.ToID == data.ID)
                    continue;

                if (!states.TryGetValue(transitionData.ToID, out var toState))
                {
                    SHLogger.Log("Could not find target state with id: " + transitionData.ToID, SHLogLevels.Error);
                    continue;
                }

                var comparisons = new List<ParameterComparison>();

                foreach (var comparisonData in transitionData.ComparisonData)
                {
                    var comparison = comparisonData.CreateComparison(parameterIDs[comparisonData.ParameterID]);
                    comparisons.Add(comparison);
                }

                var transition = new Transition(state, toState, comparisons);
                state.AddTransition(transition);
            }
        }
        #endregion

        #region States
        public bool IsLayerDefault(ILayerElement layerNode)
        {
            if (layerNode.ParentID == GraphLayer.ROOT_ID)
                return layerNode.ID == RootDefaultStateID;
            else if (TryGetData(layerNode.ParentID, out StateMachineNodeData stateMachineData))
                return stateMachineData.DefaultStateID == layerNode.ID;
            else
            {
                SHLogger.Log("Could not find parent with ID: " + layerNode.ParentID, SHLogLevels.Error);
                return false;
            }
        }

        public void SetLayerDefault(ILayerElement layerNode)
        {
            GraphLayer layer = GetLayer(layerNode);

            if (layer.IsRoot())
            {
                if (layerNode == null)
                {
                    rootDefaultStateID = string.Empty;
                    return;
                }

                if (!string.IsNullOrEmpty(rootDefaultStateID))
                {
                    if (TryGetData(rootDefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                rootDefaultStateID = layerNode.ID;
                layerNode.OnSetAsLayerDefault();
            }
            else if (TryGetData(layer.ParentID, out StateMachineNodeData stateMachineData))
            {
                if (layerNode == null)
                {
                    stateMachineData.SetInitialStateID(string.Empty);
                    return;
                }

                if (!string.IsNullOrEmpty(stateMachineData.DefaultStateID))
                {
                    if (TryGetData(stateMachineData.DefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                stateMachineData.SetInitialStateID(layerNode.ID);
                layerNode.OnSetAsLayerDefault();
            }
            else
                SHLogger.Log("Could not find layer for node with ID: " + layerNode.ID, SHLogLevels.Error);
        }

        private void ClearLayerDefault(GraphLayer layer)
        {
            if (layer.IsRoot())
            {
                if (!string.IsNullOrEmpty(rootDefaultStateID))
                {
                    if (TryGetData(rootDefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                rootDefaultStateID = string.Empty;
            }
            else if (TryGetData(layer.ParentID, out StateMachineNodeData stateMachineData))
            {
                if (!string.IsNullOrEmpty(stateMachineData.DefaultStateID))
                {
                    if (TryGetData(stateMachineData.DefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                stateMachineData.SetInitialStateID(string.Empty);
            }
        }

        public IReadOnlyList<IStateNodeData> GetStateNodes()
        {
            instanceStateNodes.Clear();
            var nodes = GetNodes();

            foreach (var node in nodes)
            {
                if (node is IStateNodeData stateNodeData)
                    instanceStateNodes.Add(stateNodeData);
            }

            return instanceStateNodes;
        }

        public StateNodeData CreateStateNodeData(Vector2 position)
        {
            var nodeData = new StateNodeData(typeof(EmptyState))
            {
                Position = position,
                Name = "New State"
            };

            AddNodeData(nodeData);
            MoveNodeToCurrentLayer(nodeData);

            if (IsDefaultAvailable(nodeData))
                SetLayerDefault(nodeData);

            return nodeData;
        }

        public StateMachineNodeData CreateStateMachineNodeData(Vector2 position)
        {
            var nodeData = new StateMachineNodeData
            {
                Position = position,
                Name = "New State Machine"
            };

            AddNodeData(nodeData);
            MoveNodeToCurrentLayer(nodeData);

            if (IsDefaultAvailable(nodeData))
                SetLayerDefault(nodeData);

            return nodeData;
        }

        public ExternalStateMachineNodeData CreateExternalStateMachineNode(Vector2 position)
        {
            var nodeData = new ExternalStateMachineNodeData
            {
                Position = position,
                Name = "New External State Machine"
            };

            AddNodeData(nodeData);
            MoveNodeToCurrentLayer(nodeData);

            if (IsDefaultAvailable(nodeData))
                SetLayerDefault(nodeData);

            return nodeData;
        }

        public StateMachineGraph GetExternalGraph(string graphID)
        {
            foreach (var stateNode in GetStateNodes())
            {
                if (stateNode is not ExternalStateMachineNodeData externalNode)
                    continue;

                if (externalNode.ExternalGraphData != null && externalNode.ExternalGraphData.ID == graphID)
                    return externalNode.ExternalGraphData;
            }

            return null;
        }

        public bool IsDefaultAvailable(ILayerElement layerNode)
        {
            // if state has a parent...
            if (!GraphLayer.IsRootID(layerNode.ParentID))
            {
                if (!TryGetData(layerNode.ParentID, out StateMachineNodeData stateMachine))
                {
                    SHLogger.Log("Could not find parent with ID: " + layerNode.ParentID, SHLogLevels.Error);
                    return false;
                }

                return string.IsNullOrEmpty(stateMachine.DefaultStateID) || !TryGetData<GraphNodeData>(stateMachine.DefaultStateID, out _);
            }

            return string.IsNullOrEmpty(rootDefaultStateID) || !TryGetData<GraphNodeData>(rootDefaultStateID, out _);
        }

        private GraphLayer GetLayer(ILayerElement element)
        {
            if (element == null || GraphLayer.IsRootID(element.ParentID))
                return CreateRootLayer();
            else if (TryGetData(element.ParentID, out StateMachineNodeData stateMachine))
                return CreateLayer(stateMachine);
            else
            {
                SHLogger.Log("Could not find layer with ID: " + element.ParentID, SHLogLevels.Error);
                return CreateRootLayer();
            }
        }
        #endregion

        #region Transitions
        public TransitionEdgeData CreateTransitionData(ITransitionable from, ITransitionable to)
        {
            var transitionData = new TransitionEdgeData(from, to);

            AddEdgeData(transitionData);

            return transitionData;
        }
        #endregion

        #region Parameters
        public IReadOnlyList<ParameterData> GetParameters()
        {
            instanceParameters.Clear();

            foreach (var parameterID in parameters)
            {
                if (TryGetData<ParameterData>(parameterID, out var parameter))
                    instanceParameters.Add(parameter);
            }

            return instanceParameters;
        }

        public void AddParameter(ParameterData parameter)
        {
            parameters.Add(parameter.ID);
            AddGraphElementData(parameter);
            ParameterDataAdded?.Invoke(parameter);
        }

        public void RemoveParameter(ParameterData parameter)
        {
            if (!parameters.Contains(parameter.ID))
            {
                Debug.LogError("Could not find parameter with ID: " + parameter.ID);
                return;
            }

            parameters.Remove(parameter.ID);
            RemoveGraphElementData(parameter);
            ParameterDataRemoved?.Invoke(parameter);
        }

        public void MoveParameterUp(ParameterData parameter)
        {
            var index = parameters.IndexOf(parameter.ID);
            var upIndex = index - 1;

            (parameters[index], parameters[upIndex]) = (parameters[upIndex], parameters[index]);
        }

        public void MoveParameterDown(ParameterData parameter)
        {
            var index = parameters.IndexOf(parameter.ID);
            var downIndex = index + 1;

            (parameters[index], parameters[downIndex]) = (parameters[downIndex], parameters[index]);
        }

        public bool IsUsableParameterName(string name)
        {
            var parameters = GetParameters();

            foreach (var parameter in parameters)
            {
                if (parameter.Name == name)
                    return false;
            }

            return true;
        }

        public void SetParameterName(ParameterData parameter, string name)
        {
            parameter.Name = name;
        }

        public void SetParameterValue<T>(ParameterData<T> parameterData, T value)
        {
            parameterData.Value = value;
        }
        #endregion
    }
}
