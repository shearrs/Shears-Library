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
        public SerializableSystemType StateType => stateType;

        public StateMachineNodeData() { }

        public StateMachineNodeData(string name, Vector2 position, string parentID, SerializableSystemType stateType)
        {
            this.name = name;
            this.position = position;
            this.parentID = parentID;
            this.stateType = stateType;
        }

        public State CreateStateInstance() => (State)Activator.CreateInstance(stateType.SystemType);

        public IReadOnlyList<string> GetTransitionIDs() => Edges;

        public void SetInitialStateID(string id) => defaultStateID = id;

        void ILayerElement.OnSetAsLayerDefault() => SetAsLayerDefault?.Invoke();

        void ILayerElement.OnRemoveLayerDefault() => RemovedAsLayerDefault?.Invoke();

        public StateMachineNodeClipboardData CopyToClipboard(CopyData data)
        {
            var transitions = new List<TransitionEdgeClipboardData>();

            foreach (var edgeID in Edges)
            {
                if (!data.GraphData.TryGetData(edgeID, out TransitionEdgeData transition))
                    continue;

                transitions.Add(new(transition));
            }

            var clipboardData = new StateMachineNodeClipboardData(this, transitions);

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
