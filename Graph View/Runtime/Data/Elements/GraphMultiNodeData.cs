using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public class GraphMultiNodeData : GraphNodeData
    {
        [SerializeField] private List<string> subNodeIDs = new();

        public IReadOnlyList<string> SubNodeIDs => subNodeIDs;

        protected event Action<string> SubNodeAdded;
        protected event Action<string> SubNodeRemoved;

        public void AddSubNode(GraphNodeData data)
        {
            subNodeIDs.Add(data.ID);
            data.SetParent(this);

            SubNodeAdded?.Invoke(data.ID);
        }

        public void RemoveSubNode(GraphNodeData data)
        {
            subNodeIDs.Remove(data.ID);
            data.SetParent(null);

            SubNodeAdded?.Invoke(data.ID);
        }
    }
}
