using Shears.GraphViews;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [Serializable]
    public class StateMachineNodeData : GraphMultiNodeData, IStateNodeData, ICopyable<StateMachineNodeClipboardData>
    {
        [SerializeField] private string defaultStateID;
        [SerializeField] private SerializableSystemType stateType = new(typeof(EmptyState));

        public event Action SetAsLayerDefault;
        public event Action RemovedAsLayerDefault;

        public string DefaultStateID => defaultStateID;

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        public void SetInitialStateID(string id) => defaultStateID = id;

        void ILayerDefaultTarget.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerDefaultTarget.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public static StateMachineNodeData PasteFromClipboard(StateMachineNodeClipboardData data, string parentID)
        {
            var stateNode = new StateMachineNodeData
            {
                name = data.Name,
                position = data.Position,
                parentID = parentID,
                stateType = data.StateType
            };

            return stateNode;
        }

        public StateMachineNodeClipboardData CopyToClipboard(CopyData data)
        {
            var clipboardData = new StateMachineNodeClipboardData(Name, Position, stateType);

            foreach (var subElementID in SubNodeIDs)
            {
                if (!data.GraphData.TryGetData<GraphElementData>(subElementID, out var subElement))
                    continue;

                if (subElement is not ICopyable copyable)
                    continue;

                clipboardData.SubElements.Add(copyable.CopyToClipboard(data));
            }

            return clipboardData;
        }

        GraphElementClipboardData ICopyable.CopyToClipboard(CopyData data)
        {
            return CopyToClipboard(data);
        }
    }
}
