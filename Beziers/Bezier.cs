using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shears.Beziers
{
    /// <summary>
    /// A bezier curve defined by a series of points with tangents.
    /// </summary>
    public class Bezier : MonoBehaviour
    {
        private const float DISTANCE_STEP = 0.001f;

        [Header("Gizmos")]
        [SerializeField, Tooltip("Whether or not to draw gizmos.")] private bool drawGizmos = true;

        [Header("Data")]
        [SerializeField, Tooltip("Whether or not the last point should connect back to the first.")] private bool closed = false;
        [SerializeField, Tooltip("The points that define the curve.")] private List<BezierPoint> points = new();

        public bool DrawGizmos => drawGizmos;
        public IReadOnlyList<BezierPoint> Points => points;

        private void Reset()
        {
            AddPoint(Vector3.zero);
            AddPoint(Vector3.right);
        }

        private void OnValidate()
        {
            foreach (var point in points)
                point.Parent = transform;
        }

        private void Awake()
        {
            foreach (var point in points)
                point.Parent = transform;
        }

        /// <summary>
        /// Adds a point to the end of the bezier curve at the given position with no rotation or tangents.
        /// </summary>
        /// <param name="position">The position to add a point at.</param>
        public void AddPoint(Vector3 position) => AddPoint(new BezierPoint(position, Quaternion.identity, Vector3.zero));

        /// <summary>
        /// Adds a defined <see cref="BezierPoint"/> to the end of the bezier curve.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void AddPoint(BezierPoint point)
        {
            point.Parent = transform;
            points.Add(point);
        }

        /// <summary>
        /// Sets the position of a point at the given index.
        /// </summary>
        /// <param name="pointIndex">The index of the point to set.</param>
        /// <param name="position">The position to set the point to.</param>
        public void SetPosition(int pointIndex, Vector3 position)
        {
            points[pointIndex].Position = position;
        }

        /// <summary>
        /// Sets the first tangent of a point at the given index.
        /// </summary>
        /// <param name="pointIndex">The index of the point to set.</param>
        /// <param name="position">The position to set the tangent to.</param>
        public void SetTangent1(int pointIndex, Vector3 position)
        {
            points[pointIndex].Tangent1 = position;
        }

        /// <summary>
        /// Sets the second tangent of a point at the given index.
        /// </summary>
        /// <param name="pointIndex">The index of the point to set.</param>
        /// <param name="position">The position to set the tangent to.</param>
        public void SetTangent2(int pointIndex, Vector3 position)
        {
            points[pointIndex].Tangent2 = position;
        }

        /// <summary>
        /// Sets the local position of the first tangent of a point at the given index.
        /// </summary>
        /// <param name="pointIndex">The index of the point to set.</param>
        /// <param name="position">The local position to set the tangent to.</param>
        public void SetLocalTangent1(int pointIndex, Vector3 position)
        {
            points[pointIndex].LocalTangent1 = position;
        }

        /// <summary>
        /// Sets the local position of the second tangent of a point at the given index.
        /// </summary>
        /// <param name="pointIndex">The index of the point to set.</param>
        /// <param name="position">The local position to set the tangent to.</param>
        public void SetLocalTangent2(int pointIndex, Vector3 position)
        {
            points[pointIndex].LocalTangent2 = position;
        }

        /// <summary>
        /// Samples the bezier curve at a given t value (0.0-1.0).
        /// </summary>
        /// <param name="t">The percentage length to sample the curve at.</param>
        /// <returns>The position of the curve at the passed sample percentage.</returns>
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

        /// <summary>
        /// Samples the bezier curve at a given distance along the curve.
        /// </summary>
        /// <param name="distance">The distance to sample the curve at.</param>
        /// <returns>The position of the curve at the passed sample distance.</returns>
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

        /// <summary>
        /// Moves a reference distance along the curve at a passed speed.
        /// </summary>
        /// <param name="speed">The speed to progress through the curve with.</param>
        /// <param name="currentDistance">The current reference distance.</param>
        /// <returns>The next moved position.</returns>
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

            if (!drawGizmos || points.Count < 2)
                return;

            Handles.color = Color.white;

            for (int i = 0; i < points.Count - 1; i++)
                Handles.DrawBezier(points[i].Position, points[i + 1].Position, points[i].Tangent2, points[i + 1].Tangent1, Color.white, Texture2D.whiteTexture, 2);

            if (closed)
                Handles.DrawBezier(points[^1].Position, points[0].Position, points[^1].Tangent2, points[0].Tangent1, Color.white, Texture2D.whiteTexture, 2);
        }
#endif
    }
}
