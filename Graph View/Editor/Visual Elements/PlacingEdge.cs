using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class PlacingEdge : VisualElement
    {
        private readonly VisualElement edgeLine;
        private readonly VisualElement arrow;

        private readonly GraphElement anchor1;
        private Vector2 anchor2;

        public PlacingEdge(GraphElement anchor1)
        {
            this.anchor1 = anchor1;

            visible = false;

            schedule.Execute(() =>
            {
                visible = true;
            }).StartingIn(10);

            schedule.Execute(() =>
            {
                DrawLine();
                MarkDirtyRepaint();
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
        }

        private void DrawLine()
        {
            Vector2 anchor1Pos = (Vector2)anchor1.transform.position + anchor1.layout.center;
            Vector2 anchor2Pos = anchor2;
            Vector2 center = 0.5f * (anchor1Pos + anchor2Pos);
            center.x -= 0.5f * layout.width;

            Vector2 heading = anchor2Pos - anchor1Pos;
            Vector2 direction = heading.normalized;
            float distance = heading.magnitude;

            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, direction);

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
