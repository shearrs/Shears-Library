using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public struct GraphLayer
    {
        private const string ROOT_NAME = "Root";
        private const string ROOT_ID = "$ROOT";

        [SerializeField] private string parentName;
        [SerializeField] private string parentID;
        [SerializeField] private Vector2 position;
        [SerializeField] private Vector2 scale;

        public readonly string ParentName => parentName;
        public readonly string ParentID => parentID;
        public Vector2 Position { readonly get => position; set => position = value; }
        public Vector2 Scale { readonly get => scale; set => scale = value; }

        public GraphLayer(GraphMultiNodeData parent)
        {
            position = Vector2.zero;
            scale = Vector2.one;

            if (parent != null)
            {
                parentName = parent.Name;
                parentID = parent.ID;
            }
            else
            {
                parentName = ROOT_NAME;
                parentID = ROOT_ID;
            }
        }

        public GraphLayer(Vector2 position, Vector2 scale, GraphMultiNodeData parent)
        {
            this.position = position;
            this.scale = scale;

            if (parent != null)
            {
                parentName = parent.Name;
                parentID = parent.ID;
            }
            else
            {
                parentName = ROOT_NAME;
                parentID = ROOT_ID;
            }
        }

        public readonly bool IsRoot() => parentID == ROOT_ID;
    }
}
