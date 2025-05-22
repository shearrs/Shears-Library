using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shears.Beziers
{
    public class Bezier : MonoBehaviour
    {
        private const float DISTANCE_STEP = 0.001f;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;

        [Header("Data")]
        [SerializeField] private bool closed = false;
        [SerializeField] private List<BezierPoint> points = new();

        public IReadOnlyList<BezierPoint> Points => points;

        private void OnValidate()
        {
            foreach (var point in points)
                point.Parent = transform;
        }

        public Vector3 Sample(float t)
        {
            if (points.Count < 2)
                return Vector3.zero;

            if (!Mathf.Approximately(t, 1))
                t %= 1;

            int pointCount = points.Count;

            if (closed)
                pointCount++;

            int pointIndex = Mathf.Min(Mathf.FloorToInt(t * (pointCount - 1)), pointCount - 2);
            float localT = t * (pointCount - 1) - pointIndex;

            Vector3 finalPosition;
            BezierPoint point1;
            BezierPoint point2;

            if (pointIndex >= points.Count - 1)
            {
                point1 = points[^1];
                point2 = points[0];
            }
            else
            {
                point1 = points[pointIndex];
                point2 = points[pointIndex + 1];
            }

            Vector3 t1 = Vector3.Lerp(point1.Position, point1.Tangent2, localT);
            Vector3 t2 = Vector3.Lerp(point1.Tangent2, point2.Tangent1, localT);
            Vector3 t3 = Vector3.Lerp(point2.Tangent1, point2.Position, localT);

            Vector3 b1 = Vector3.Lerp(t1, t2, localT);
            Vector3 b2 = Vector3.Lerp(t2, t3, localT);

            finalPosition = Vector3.Lerp(b1, b2, localT);

            return finalPosition;
        }

        public Vector3 SampleDistance(float distance)
        {
            if (points.Count < 2)
                return Vector3.zero;
            if (closed)
                distance %= GetLength();

            float totalDistance = 0;
            float step = DISTANCE_STEP;
            Vector3 lastPoint = Sample(0);
            Vector3 currentPoint;

            for (float t = step; t <= 1; t += step)
            {
                currentPoint = Sample(t);
                totalDistance += Vector3.Distance(lastPoint, currentPoint);

                if (totalDistance >= distance)
                    return currentPoint;

                lastPoint = currentPoint;
            }

            return lastPoint;
        }

        public Vector3 MoveAlong(float speed, ref float currentDistance)
        {
            currentDistance += speed * Time.deltaTime;

            Vector3 position = SampleDistance(currentDistance);

            return position;
        }

        private float GetLength()
        {
            float length = 0;
            float step = DISTANCE_STEP;
            Vector3 lastPoint = Sample(0);
            Vector3 currentPoint;

            for (float t = step; t <= 1; t += step)
            {
                currentPoint = Sample(t);
                length += Vector3.Distance(lastPoint, currentPoint);
                lastPoint = currentPoint;
            }

            if (closed)
            {
                currentPoint = Sample(0);
                length += Vector3.Distance(lastPoint, currentPoint);
            }

            return length;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!drawGizmos || points.Count < 2)
                return;

            Handles.color = Color.white;

            for (int i = 0; i < points.Count - 1; i++)
                Handles.DrawBezier(points[i].Position, points[i + 1].Position, points[i].Tangent2, points[i + 1].Tangent1, Color.white, Texture2D.whiteTexture, 2);

            if (closed)
                Handles.DrawBezier(points[^1].Position, points[0].Position, points[^1].Tangent2, points[0].Tangent1, Color.white, Texture2D.whiteTexture, 2);
#endif
        }
    }
}
