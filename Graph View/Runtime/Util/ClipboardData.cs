using System;
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
}
