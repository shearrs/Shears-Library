using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public abstract class GraphData : ScriptableObject
    {
        [Header("Transform")]
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 scale = Vector2.one;

        [Header("Elements")]
        [SerializeField] private GraphElementData selection;
        [SerializeField] private List<GraphNodeData> nodeData = new();
        [SerializeField] private List<GraphEdgeData> edgeData = new();
        [SerializeField] private Stack<GraphNodeData> nodePath = new();

        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Scale { get => scale; set => scale = value; }
        public GraphElementData Selection { get => selection; set => selection = value; }

        public IReadOnlyList<GraphNodeData> NodeData => nodeData;
        public IReadOnlyList<GraphEdgeData> EdgeData => edgeData;
        public IReadOnlyCollection<GraphNodeData> NodePath => nodePath;
    }
}
