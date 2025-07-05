using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphData : ScriptableObject
    {
        private Vector2 position;
        private Vector2 scale;
        private float zoom;
        private GraphElementData selection;

        private readonly List<GraphNodeData> nodeData = new();
        private readonly List<GraphEdgeData> edgeData = new();
        private readonly Stack<GraphNodeData> nodePath = new();

        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }
        public float Zoom { get => zoom; set => zoom = value; }
        public GraphElementData Selection { get => selection; set => selection = value; }

        public IReadOnlyList<GraphNodeData> NodeData => nodeData;
        public IReadOnlyList<GraphEdgeData> EdgeData => edgeData;
        public IReadOnlyCollection<GraphNodeData> NodePath => nodePath;
    }
}
