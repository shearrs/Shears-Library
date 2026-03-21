using Shears.GraphViews;
using Shears.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class TransitionEdgeClipboardData : GraphElementClipboardData
    {
        [SerializeField]
        private string fromID;

        [SerializeField]
        private string toID;

        [SerializeReference]
        private List<TransitionData> transitionData = new();

        public TransitionEdgeClipboardData(TransitionEdgeData transition) : base(transition.ID)
        {
            fromID = transition.FromID;
            toID = transition.ToID;
            transitionData.AddRange(transition.TransitionData);
        }

        public override GraphElementData Paste(PasteData data)
        {
            if (data.Mapping.ContainsKey(OriginalID))
                return null;

            var stateGraph = data.GraphData as StateMachineGraph;

            if (!data.Mapping.TryGetValue(fromID, out var from))
                return null;

            if (!data.Mapping.TryGetValue(toID, out var to))
                return null;

            if (from is not ITransitionable transitionableFrom)
            {
                SHLogger.Log($"'From' does not implement {nameof(ITransitionable)}!", SHLogLevels.Error);
                return null;
            }
            if (to is not ITransitionable transitionableTo)
            {
                SHLogger.Log($"'To' does not implement {nameof(ITransitionable)}!", SHLogLevels.Error);
                return null;
            }

            var transition = stateGraph.CreateTransitionEdgeData(transitionableFrom, transitionableTo);

            foreach (var tData in transitionData)
                transition.AddTransitionData(tData);

            data.Mapping.Add(OriginalID, transition);

            return transition;
        }
    }
}
