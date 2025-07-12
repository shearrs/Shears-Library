using System;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public abstract class GraphEdgeData : GraphElementData
    {
        [SerializeField] private string fromID;
        [SerializeField] private string toID;

        public string FromID => fromID;
        public string ToID => toID;

        public event Action Selected;
        public event Action Deselected;

        public GraphEdgeData(string fromID, string toID)
        {
            this.fromID = fromID;
            this.toID = toID;
        }

        public override void Select()
        {
            Selected?.Invoke();
        }

        public override void Deselect()
        {
            Deselected?.Invoke();
        }
    }
}
