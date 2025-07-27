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
        private readonly List<IStateNodeData> instanceStateNodes = new();
        private readonly List<ParameterData> instanceParameters = new();

        public string RootDefaultStateID => rootDefaultStateID;

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
                        SetLayerDefault(null);
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

        protected override GraphNodeData CreateNodeFromClipboard(GraphNodeClipboardData data, string parentID = "")
        {
            if (parentID == "")
                parentID = Layers[^1].ParentID;

            if (data is StateNodeClipboardData stateNodeData)
            {
                var stateNode = StateNodeData.PasteFromClipboard(stateNodeData, parentID);

                AddNodeData(stateNode);

                if (parentID == Layers[^1].ParentID)
                    MoveNodeToCurrentLayer(stateNode);

                if (IsDefaultAvailable(stateNode))
                    SetLayerDefault(stateNode);

                return stateNode;
            }
            else if (data is StateMachineNodeClipboardData stateMachineNodeData)
            {
                var stateMachineNode = StateMachineNodeData.PasteFromClipboard(stateMachineNodeData, parentID);

                AddNodeData(stateMachineNode);

                if (parentID == Layers[^1].ParentID)
                    MoveNodeToCurrentLayer(stateMachineNode);

                if (IsDefaultAvailable(stateMachineNode))
                    SetLayerDefault(stateMachineNode);

                foreach (var subNodeID in stateMachineNodeData.SubNodeIDs)
                {
                    if (!TryGetData(subNodeID, out GraphNodeData subNode))
                    {
                        SHLogger.Log("Could not find sub-node with ID: " + subNodeID, SHLogLevels.Error);
                        continue;
                    }
                    
                    var clipboard = (GraphNodeClipboardData)subNode.CopyToClipboard();
                    var copy = CreateNodeFromClipboard(clipboard, stateMachineNode.ID);

                    stateMachineNode.AddSubNode(copy);
                }

                return stateMachineNode;
            }

            return null;
        }

        #region States
        public bool IsLayerDefault(IStateNodeData stateNode)
        {
            if (stateNode.ParentID == GraphLayer.ROOT_ID)
                return stateNode.ID == RootDefaultStateID;
            else if (TryGetData(stateNode.ParentID, out StateMachineNodeData stateMachineData))
                return stateMachineData.DefaultStateID == stateNode.ID;
            else
            {
                SHLogger.Log("Could not find parent with ID: " + stateNode.ParentID, SHLogLevels.Error);
                return false;
            }
        }

        public void SetLayerDefault(IStateNodeData stateNodeData)
        {
            if (stateNodeData == null)
            {
                rootDefaultStateID = string.Empty;
                return;
            }

            if (GraphLayer.IsRootID(stateNodeData.ParentID))
            {
                if (!string.IsNullOrEmpty(rootDefaultStateID))
                {
                    if (TryGetData(rootDefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                rootDefaultStateID = stateNodeData.ID;
                stateNodeData.OnSetAsLayerDefault();
            }
            else if (TryGetData(stateNodeData.ParentID, out StateMachineNodeData stateMachineData))
            {
                if (!string.IsNullOrEmpty(stateMachineData.DefaultStateID))
                {
                    if (TryGetData(stateMachineData.DefaultStateID, out GraphNodeData defaultNode) && defaultNode is IStateNodeData defaultState)
                        defaultState.OnRemoveLayerDefault();
                }

                stateMachineData.SetInitialStateID(stateNodeData.ID);
                stateNodeData.OnSetAsLayerDefault();
            }
            else
                SHLogger.Log("Could not find layer for node with ID: " + stateNodeData.ID, SHLogLevels.Error);
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
            var nodeData = new StateNodeData
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

            return nodeData;
        }

        private bool IsDefaultAvailable(IStateNodeData stateNode)
        {
            // if state has a parent...
            if (!GraphLayer.IsRootID(stateNode.ParentID))
            {
                if (!TryGetData(stateNode.ParentID, out StateMachineNodeData stateMachine))
                {
                    SHLogger.Log("Could not find parent with ID: " + stateNode.ParentID, SHLogLevels.Error);
                    return false;
                }

                return string.IsNullOrEmpty(stateMachine.DefaultStateID) || !TryGetData<GraphNodeData>(stateMachine.DefaultStateID, out _);
            }

            return string.IsNullOrEmpty(rootDefaultStateID) || !TryGetData<GraphNodeData>(rootDefaultStateID, out _);
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
