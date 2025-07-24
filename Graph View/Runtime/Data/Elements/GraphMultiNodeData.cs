using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public abstract class GraphMultiNodeData : GraphNodeData
    {
        [SerializeField] private List<string> subNodeIDs = new();

        public IReadOnlyList<string> SubNodeIDs => subNodeIDs;

        public void AddSubNode(GraphNodeData data)
        {
            subNodeIDs.Add(data.ID);
            data.SetParent(this);
        }

        public void RemoveSubNode(GraphNodeData data)
        {
            subNodeIDs.Remove(data.ID);
        }
    }
}
