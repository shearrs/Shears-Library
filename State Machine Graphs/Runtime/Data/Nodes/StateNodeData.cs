using Shears.GraphViews;
using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateNodeData : GraphNodeData, IStateNodeData, ICopyable<StateNodeClipboardData>
    {
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        IReadOnlyList<string> ITransitionable.GetTransitionIDs() => Edges;

        State IStateNodeData.CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        void ILayerDefaultTarget.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerDefaultTarget.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public static StateNodeData PasteFromClipboard(StateNodeClipboardData data, string parentID)
        {
            var stateNode = new StateNodeData
            {
                name = data.Name,
                position = data.Position,
                parentID = parentID,
                stateType = data.StateType
            };

            return stateNode;
        }

        public StateNodeClipboardData CopyToClipboard(CopyData data) => new(Name, Position, stateType);

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }
    }
}
