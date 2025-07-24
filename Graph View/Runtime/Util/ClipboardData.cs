using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public class GraphNodeClipboardData : GraphElementClipboardData
    {
        [SerializeField] private string name;
        [SerializeField] private Vector2 position;
        [SerializeField] private List<string> edges = new();

        public string Name => name;
        public Vector2 Position => position;
        public IReadOnlyList<string> Edges => edges;

        public GraphNodeClipboardData(string name, Vector2 position, List<string> edges)
        {
            this.name = name;
            this.position = position;
            this.edges = edges ?? new List<string>();
        }
    }
}
