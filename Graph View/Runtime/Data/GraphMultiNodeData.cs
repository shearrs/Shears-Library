using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public class GraphMultiNodeData : GraphNodeData
    {
        [SerializeField] private List<GraphReference<GraphNodeData>> subNodes = new();

        private readonly List<GraphNodeData> instanceNodes = new();

        public IReadOnlyList<GraphNodeData> GetSubNodes()
        {
            instanceNodes.Clear();

            foreach (var reference in subNodes)
            {
                if (reference.Data != null)
                    instanceNodes.Add(reference.Data);
            }

            return instanceNodes;
        }

        public void AddSubNode(GraphNodeData data)
        {
            subNodes.Add(new(data));

            data.SetParent(this);
        }
    }
}
