using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode : GraphElement, IEdgeAnchorable
    {
        private readonly GraphNodeData data;
        private readonly GraphView graphView;

        string IEdgeAnchorable.ID => data.ID;
        GraphElement IEdgeAnchorable.Element => this;

        protected GraphNode(GraphNodeData data, GraphView graphView)
        {
            this.data = data;
            this.graphView = graphView;

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

        public bool HasConnectionTo(IEdgeAnchorable anchor)
        {
            foreach (string edgeID in data.Edges)
            {
                var edge = graphView.GetEdge(edgeID);
                var edgeData = edge.GetData() as GraphEdgeData;

                if (edgeData.ToID == anchor.ID)
                    return true;
            }

            return false;
        }
    }
}
