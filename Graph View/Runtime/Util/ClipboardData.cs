using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public abstract class GraphNodeClipboardData : GraphElementClipboardData
    {
        private const float COPY_OFFSET = -150f;

        [SerializeField] private string name;
        [SerializeField] private Vector2 position;

        public string Name => name;
        public Vector2 Position => position;

        public GraphNodeClipboardData(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
            this.position.y += COPY_OFFSET;
        }
    }

    [Serializable]
    public abstract class GraphMultiNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeReference] private List<GraphElementClipboardData> subElements = new();

        public List<GraphElementClipboardData> SubElements => subElements;

        public GraphMultiNodeClipboardData(string name, Vector2 position)
            : base(name, position)
        {
        }
    }
}