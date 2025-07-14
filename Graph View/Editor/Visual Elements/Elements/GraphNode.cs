using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode : GraphElement, IEdgeAnchorable, ISelectable
    {
        private readonly GraphNodeData data;
        private readonly SerializedProperty nodeProperty;
        private readonly GraphView graphView;

        string IEdgeAnchorable.ID => data.ID;
        GraphElement IEdgeAnchorable.Element => this;

        protected GraphNode(GraphNodeData data, SerializedProperty nodeProperty, GraphView graphView)
        {
            this.data = data;
            this.nodeProperty = nodeProperty;
            this.graphView = graphView;

            AddToClassList(GraphViewEditorUtil.GraphNodeClassName);

            var nameProp = nodeProperty.FindPropertyRelative("name");
            var nameLabel = new Label()
            {
                pickingMode = PickingMode.Ignore
            };

            nameLabel.BindProperty(nameProp);

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

        public void Select()
        {
            AddToClassList(GraphViewEditorUtil.GraphNodeSelectedClassName);
        }

        public void Deselect()
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

        public bool HasConnectionFrom(IEdgeAnchorable anchor)
        {
            foreach (string edgeID in data.Edges)
            {
                var edge = graphView.GetEdge(edgeID);
                var edgeData = edge.GetData() as GraphEdgeData;

                if (edgeData.FromID == anchor.ID)
                    return true;
            }

            return false;
        }
    }
}
