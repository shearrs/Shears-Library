using Shears.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphEdge : GraphElement, ISelectable
    {
        private const float DOUBLE_EDGE_SPACING = 16f;

        private readonly GraphEdgeData data;
        private readonly VisualElement edgeLine;
        private readonly VisualElement arrow;

        private bool geometryInitialized = false;
        private IEdgeAnchorable anchor1;
        private IEdgeAnchorable anchor2;

        public GraphEdge(GraphEdgeData data, IEdgeAnchorable anchor1, IEdgeAnchorable anchor2)
        {
            this.data = data;
            this.anchor1 = anchor1;
            this.anchor2 = anchor2;

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

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        ~GraphEdge()
        {
            data.Selected -= Select;
            data.Deselected -= Deselect;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            DrawLine();
        }

        private void DrawLine()
        {
            Vector2 anchor1Pos = (Vector2)anchor1.Element.transform.position + anchor1.Element.layout.center;
            Vector2 anchor2Pos = (Vector2)anchor2.Element.transform.position + anchor2.Element.layout.center;
            Vector2 center = 0.5f * (anchor1Pos + anchor2Pos);

            Vector2 heading = anchor2Pos - anchor1Pos;
            float distance = heading.magnitude;
            Vector2 direction = heading.normalized;

            float angle = Vector2.SignedAngle(Vector2.right, direction);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (!float.IsNaN(layout.width))
            {
                center.x -= 0.5f * layout.width;

                if (anchor1.HasConnectionFrom(anchor2))
                    center += (Vector2)(rotation * (Vector2.up * DOUBLE_EDGE_SPACING));
                
                if (layout.width != 0 && layout.height != 0 && !geometryInitialized)
                {
                    schedule.Execute(() =>
                    {
                        DrawLine();
                    }).Every(1);

                    geometryInitialized = true;
                }
            }

            style.width = distance;
            transform.position = center;
            transform.rotation = rotation;
        }

        protected void SetAnchor1(IEdgeAnchorable anchor)
        {
            anchor1 = anchor;
        }

        protected void SetAnchor2(IEdgeAnchorable anchor)
        {
            anchor2 = anchor;
        }

        public void Select()
        {
            edgeLine.AddToClassList(GraphViewEditorUtil.EdgeLineSelectedClassName);
            arrow.AddToClassList(GraphViewEditorUtil.EdgeArrowSelectedClassName);
        }

        public void Deselect()
        {
            edgeLine.RemoveFromClassList(GraphViewEditorUtil.EdgeLineSelectedClassName);
            arrow.RemoveFromClassList(GraphViewEditorUtil.EdgeArrowSelectedClassName);
        }

        public override GraphElementData GetData() => data;
    }
}
