using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [Serializable]
    public class GraphNodeClipboardData : GraphElementClipboardData
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
    public class GraphMultiNodeClipboardData : GraphNodeClipboardData
    {
        [SerializeField] private List<string> subNodeIDs = new();

        public IReadOnlyList<string> SubNodeIDs => subNodeIDs;

        public GraphMultiNodeClipboardData(string name, Vector2 position, List<string> subNodeIDs)
            : base(name, position)
        {
            this.subNodeIDs = subNodeIDs;
        }
    }
}