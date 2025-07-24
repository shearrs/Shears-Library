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
        [SerializeField] protected List<string> edges = new();
        [SerializeField] private string parentID = GraphLayer.ROOT_ID;

        public string Name { get => name; set => name = value; }
        public Vector2 Position { get => position; set => position = value; }
        public string ParentID => parentID;
        public IReadOnlyList<string> Edges => edges;

        public event Action Selected;
        public event Action Deselected;

        public void SetParent(GraphMultiNodeData parent)
        {
            if (parent == null)
                parentID = GraphLayer.ROOT_ID;
            else
                parentID = parent.ID;
        }

        public void AddEdge(GraphEdgeData edge)
        {
            if (edges.Contains(edge.ID))
            {
                Debug.LogError("Node already contains edge with ID: " + edge.ID);
                return;
            }

            edges.Add(edge.ID);
        }

        public void RemoveEdge(GraphEdgeData edge)
        {
            edges.Remove(edge.ID);
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
