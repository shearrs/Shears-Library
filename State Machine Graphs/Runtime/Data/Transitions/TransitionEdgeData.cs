using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    [System.Serializable]
    public class TransitionEdgeData : GraphEdgeData
    {
        [SerializeReference]
        private List<TransitionData> transitionData = new();

        public IReadOnlyList<TransitionData> TransitionData => transitionData;

        public TransitionEdgeData(ITransitionable from, ITransitionable to) : base(from.ID, to.ID)
        {
            transitionData.Add(new TransitionData());
        }

        public void AddTransitionData(TransitionData data)
        {
            transitionData.Add(data);
        }
    }
}
