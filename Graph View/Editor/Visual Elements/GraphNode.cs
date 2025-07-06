using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode<T> : GraphElement<T> where T : GraphNodeData
    {
        public GraphNode(T data) : base(data)
        {
            AddToClassList(GraphViewEditorUtil.GraphNodeClassName);

            var nameLabel = new Label(data.Name)
            {
                pickingMode = PickingMode.Ignore
            };

            Add(nameLabel);

            transform.position = data.Position;

            data.Selected += Select;
            data.Deselected += Deselect;
        }

        ~GraphNode()
        {
            Data.Selected -= Select;
            Data.Deselected -= Deselect;
        }

        public override void Select()
        {
            AddToClassList(GraphViewEditorUtil.GraphNodeSelectedClassName);
        }

        public override void Deselect()
        {
            RemoveFromClassList(GraphViewEditorUtil.GraphNodeSelectedClassName);
        }
    }
}
