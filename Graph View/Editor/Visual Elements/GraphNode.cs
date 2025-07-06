using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode : GraphElement
    {
        private readonly GraphNodeData data;

        protected GraphNode(GraphNodeData data)
        {
            this.data = data;

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
            data.Selected -= Select;
            data.Deselected -= Deselect;
        }

        public override void Select()
        {
            AddToClassList(GraphViewEditorUtil.GraphNodeSelectedClassName);
        }

        public override void Deselect()
        {
            RemoveFromClassList(GraphViewEditorUtil.GraphNodeSelectedClassName);
        }

        public override GraphElementData GetData() => data;
    }
}
