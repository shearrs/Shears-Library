using UnityEditor;
using UnityEngine;

namespace Shears.Beziers.Editor
{
    [CustomEditor(typeof(Bezier))]
    public class BezierEditor : UnityEditor.Editor
    {
        private const float HANDLE_SIZE = 0.1f;

        protected virtual void OnSceneGUI()
        {
            var bezier = (Bezier)target;

            float biggestScale = Mathf.Max(bezier.transform.localScale.x, bezier.transform.localScale.y, bezier.transform.localScale.z);

            foreach (var point in bezier.Points)
            {
                EditorGUI.BeginChangeCheck();

                CreatePointHandles(bezier, point);
            }
        }

        private void CreatePointHandles(Bezier bezier, BezierPoint point)
        {
            Handles.color = Color.red;
            Handles.DrawDottedLine(point.Tangent1, point.Tangent2, 2f);

            if (point.LocalPosition == Vector3.zero)
                Tools.hidden = true;
            else
                Tools.hidden = false;

            Vector3 newPos = point.Position;
            Vector3 newTangent1 = point.Tangent1;
            Vector3 newTangent2 = point.Tangent2;
            Quaternion newRotation = point.Rotation;

            var currentTool = Tools.current;

            if (currentTool == Tool.Rect || currentTool == Tool.View || currentTool == Tool.Scale)
            {
                Handles.color = Color.cyan;
                newPos = Snap(Handles.FreeMoveHandle(point.Position, HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap));

                Handles.color = Color.red;
                newTangent1 = Snap(Handles.FreeMoveHandle(point.Tangent1, 0.5f * HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap));
                newTangent2 = Snap(Handles.FreeMoveHandle(point.Tangent2, 0.5f * HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap));
            }
            else if (currentTool == Tool.Move)
            {
                var rotation = Tools.pivotRotation == PivotRotation.Local ? 
                    point.LocalRotation : Quaternion.identity;

                Handles.color = Color.cyan;
                Handles.FreeMoveHandle(point.Position, HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap);

                Handles.color = Color.red;
                Handles.FreeMoveHandle(point.Tangent1, 0.5f * HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap);
                Handles.FreeMoveHandle(point.Tangent2, 0.5f * HandleUtility.GetHandleSize(point.Position) * HANDLE_SIZE, Vector3.zero, Handles.CircleHandleCap);

                newPos = Snap(Handles.PositionHandle(point.Position, rotation));
                newTangent1 = Snap(Handles.PositionHandle(point.Tangent1, rotation));
                newTangent2 = Snap(Handles.PositionHandle(point.Tangent2, rotation));
            }
            else if (currentTool == Tool.Rotate || currentTool == Tool.Transform)
            {
                newRotation = Handles.RotationHandle(point.Rotation, point.Position);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezier, "Move Bezier Point");

                float dist = (point.Position - newPos).sqrMagnitude;

                if (dist > float.Epsilon)
                {
                    point.Position = newPos;
                    return;
                }

                float tangent1Dist = (point.Tangent1 - newTangent1).sqrMagnitude;
                float tangent2Dist = (point.Tangent2 - newTangent2).sqrMagnitude;

                if (tangent1Dist > float.Epsilon)
                {
                    point.Tangent1 = newTangent1;
                    return;
                }

                if (tangent2Dist > float.Epsilon)
                {
                    point.Tangent2 = newTangent2;
                    return;
                }

                if (Quaternion.Angle(point.Rotation, newRotation) > float.Epsilon)
                    point.Rotation = newRotation;
            }
        }

        private Vector3 Snap(Vector3 position) => Snapping.Snap(position, EditorSnapSettings.gridSize);
    }
}
