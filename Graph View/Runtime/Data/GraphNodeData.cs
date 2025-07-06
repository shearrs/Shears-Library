using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphNodeData : GraphElementData
    {
        [SerializeField] private string name;
        [SerializeField] private Vector2 position;
        [SerializeField] private readonly List<GraphReference<GraphEdgeData>> edges = new();

        private readonly List<GraphEdgeData> instanceEdges = new();

        public string Name { get => name; set => name = value; }
        public Vector2 Position { get => position; set => position = value; }

        public IReadOnlyList<GraphEdgeData> GetEdges()
        {
            instanceEdges.Clear();

            foreach (var reference in edges)
            {
                if (reference.Data != null)
                    instanceEdges.Add(reference.Data);
            }

            return instanceEdges;
        }
    }
}
