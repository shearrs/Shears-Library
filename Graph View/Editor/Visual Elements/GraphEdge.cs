using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphEdge : GraphElement
    {
        private readonly GraphEdgeData data;
        private readonly VisualElement edgeLine;
        private readonly VisualElement arrow;

        private GraphElement anchor1;
        private GraphElement anchor2;

        public GraphEdge(GraphEdgeData data, GraphElement anchor1, GraphElement anchor2)
        {
            this.data = data;
            this.anchor1 = anchor1;
            this.anchor2 = anchor2;

            //visible = false;

            //schedule.Execute(() =>
            //{
            //    visible = true;
            //}).StartingIn(10);

            schedule.Execute(() =>
            {
                Redraw();
            }).Every(1);

            edgeLine = new()
            {
                pickingMode = PickingMode.Ignore
            };

            arrow = new()
            {
                pickingMode = PickingMode.Ignore
            };

            Add(edgeLine);
            Add(arrow);

            AddToClassList(GraphViewEditorUtil.EdgeClassName);
            edgeLine.AddToClassList(GraphViewEditorUtil.EdgeLineClassName);
            arrow.AddToClassList(GraphViewEditorUtil.EdgeArrowClassName);

            data.Selected += Select;
            data.Deselected += Deselect;
        }
        
        ~GraphEdge()
        {
            data.Selected -= Select;
            data.Deselected -= Deselect;
        }

        private void Redraw()
        {
            DrawLine();
            MarkDirtyRepaint();
        }

        // seems like the first frame (or so) is placing it at like local positions? it has no offset other than the different between anchor1 and 2
        private void DrawLine()
        {
            Vector2 anchor1Pos = (Vector2)anchor1.transform.position + anchor1.layout.center;
            Vector2 anchor2Pos = (Vector2)anchor2.transform.position + anchor2.layout.center;

            Vector2 center = 0.5f * (anchor1Pos + anchor2Pos);
            center.x -= 0.5f * layout.width;

            Vector2 heading = anchor2Pos - anchor1Pos;
            float distance = heading.magnitude;
            Vector2 direction = heading.normalized;

            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, direction);

            style.width = distance;

            transform.position = center;
            transform.rotation = rotation;
        }

        protected void SetAnchor1(GraphElement anchor)
        {
            anchor1 = anchor;
        }

        protected void SetAnchor2(GraphElement anchor)
        {
            anchor2 = anchor;
        }

        public override void Select()
        {
            edgeLine.AddToClassList(GraphViewEditorUtil.EdgeLineSelectedClassName);
            arrow.AddToClassList(GraphViewEditorUtil.EdgeArrowSelectedClassName);
        }

        public override void Deselect()
        {
            edgeLine.RemoveFromClassList(GraphViewEditorUtil.EdgeLineSelectedClassName);
            arrow.RemoveFromClassList(GraphViewEditorUtil.EdgeArrowSelectedClassName);
        }

        public override GraphElementData GetData() => data;
    }
}
