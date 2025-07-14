using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class PlacingEdge : VisualElement
    {
        private readonly VisualElement edgeLine;
        private readonly VisualElement arrow;

        private readonly IEdgeAnchorable anchor1;
        private bool geometryInitialized = false;
        private Vector2 anchor2;

        public PlacingEdge(IEdgeAnchorable anchor1)
        {
            this.anchor1 = anchor1;

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

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            DrawLine();
        }

        private void DrawLine()
        {
            Vector2 anchor1Pos = (Vector2)anchor1.Element.transform.position + anchor1.Element.layout.center;
            Vector2 anchor2Pos = anchor2;
            Vector2 center = 0.5f * (anchor1Pos + anchor2Pos);

            Vector2 heading = anchor2Pos - anchor1Pos;
            Vector2 direction = heading.normalized;
            float distance = heading.magnitude;

            float angle = Vector2.SignedAngle(Vector2.right, direction);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (!float.IsNaN(layout.width))
            {
                center.x -= 0.5f * layout.width;

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

        public void SetAnchor2(Vector2 anchor)
        {
            anchor2 = anchor;
        }
    }
}
