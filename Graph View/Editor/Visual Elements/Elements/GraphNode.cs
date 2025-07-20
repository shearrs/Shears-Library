using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphNode : GraphElement, IEdgeAnchorable, ISelectable
    {
        private readonly GraphNodeData data;
        private readonly GraphView graphView;

        string IEdgeAnchorable.ID => data.ID;
        GraphElement IEdgeAnchorable.Element => this;

        // pass the graphData
        // get elements prop
        // get name prop
        // bind to name prop
        // track property value of elements prop
        // on change, rebind name prop

        protected GraphNode(GraphNodeData data, GraphView graphView, GraphData graphData)
        {
            this.data = data;
            this.graphView = graphView;

            AddToClassList(GraphViewEditorUtil.GraphNodeClassName);

            // every time the elements list changes, these need to update their name bindings
            var graphSO = new SerializedObject(graphData);
            var elementsProp = graphSO.FindProperty("graphElements").FindPropertyRelative("values");

            var nameProp = GraphViewEditorUtil.GetElementProp(graphData, data.ID).FindPropertyRelative("name");
            var nameLabel = new Label()
            {
                pickingMode = PickingMode.Ignore
            };

            void rebind(SerializedProperty prop)
            {
                var nameProp = GraphViewEditorUtil.GetElementProp(graphData, data.ID).FindPropertyRelative("name");

                nameLabel.Unbind();
                nameLabel.BindProperty(nameProp);
            }

            nameLabel.BindProperty(nameProp);
            nameLabel.TrackPropertyValue(elementsProp, rebind);

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
