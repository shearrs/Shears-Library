using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode<T> : GraphElement<T> where T : GraphNodeData
    {
        public GraphNode(T data) : base(data)
        {
            AddToClassList(GraphViewEditorUtil.GraphNodeClassName);

            var nameLabel = new Label(data.Name);
            Add(nameLabel);

            transform.position = data.Position;
        }
    }
}
