using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public abstract class GraphNodeData : GraphElementData
    {
        [SerializeField] private string name;
        [SerializeField] private Vector2 position;
        [SerializeField] private readonly List<GraphReference<GraphEdgeData>> edges = new();
        [SerializeField] private string parentID;

        private readonly List<GraphEdgeData> instanceEdges = new();

        public string Name { get => name; set => name = value; }
        public Vector2 Position { get => position; set => position = value; }
        public string ParentID => parentID;

        public event Action Selected;
        public event Action Deselected;

        public void SetParent(GraphMultiNodeData parent)
        {
            parentID = parent.ID;
        }

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
