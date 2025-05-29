using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox3D : HitBody3D
    {
        [Flags]
        private enum GizmoMode
        {
            Everything = -1,
            None = 0,
            Hitbox = 1 << 0,
            Rays = 1 << 1,
            Hits = 1 << 2,
            HitAverages = 1 << 3,
            Activity = 1 << 4
        }

        [Header("Gizmos")]
        [SerializeField] private GizmoMode gizmoMode = GizmoMode.Everything & ~GizmoMode.Activity;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerFace = 8;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 orientation = Vector3.zero;
        [SerializeField] private Vector3 size = Vector3.one;

        private readonly Dictionary<Collider, List<RaycastHit>> recentHits = new();
        private readonly RaycastHit[] results = new RaycastHit[100];

        private Vector3 Center => transform.position + center;
        private Quaternion Orientation => transform.rotation * Quaternion.Euler(orientation);
        private Vector3 Size => Vector3.Scale(size, transform.lossyScale);

        public Vector3 GetAverageHitPosition(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            Vector3 averageHitPosition = Vector3.zero;

            foreach (var hit in recentHits[collider])
                averageHitPosition += hit.point;

            averageHitPosition /= recentHits[collider].Count;

            return averageHitPosition;
        }

        public Vector3 GetAverageSurfacePosition(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            var noScaleMatrix = Matrix4x4.TRS(Center, Orientation, Vector3.one).inverse;
            var fullMatrix = Matrix4x4.TRS(Center, Orientation, Size).inverse;

            Vector3 averagePosition = GetAverageHitPosition(collider);

            Vector3 positionDistances = noScaleMatrix.MultiplyPoint3x4(averagePosition);
            positionDistances = new Vector3(Mathf.Abs(positionDistances.x), Mathf.Abs(positionDistances.y), Mathf.Abs(positionDistances.z));
            positionDistances = new((size.x * 0.5f) - positionDistances.x, (size.y * 0.5f) - positionDistances.y, (size.z * 0.5f) - positionDistances.z);

            Vector3 localAveragePosition = fullMatrix.MultiplyPoint3x4(averagePosition);
            Vector3 localAbsAveragePosition = new(Mathf.Abs(localAveragePosition.x), Mathf.Abs(localAveragePosition.y), Mathf.Abs(localAveragePosition.z));
            Vector3 surfacePosition;

            float min = Mathf.Min(positionDistances.x, positionDistances.y, positionDistances.z);

            if (min == positionDistances.x)
                surfacePosition = localAveragePosition + (Vector3.right * ((.5f - localAbsAveragePosition.x) * Mathf.Sign(localAveragePosition.x)));
            else if (min == positionDistances.y)
                surfacePosition = localAveragePosition + (Vector3.up * ((.5f - localAbsAveragePosition.y) * Mathf.Sign(localAveragePosition.y)));
            else
                surfacePosition = localAveragePosition + (Vector3.forward * ((.5f - localAbsAveragePosition.z) * Mathf.Sign(localAveragePosition.z)));

            return fullMatrix.inverse.MultiplyPoint3x4(surfacePosition);
        }

        public Vector3 GetAverageSurfaceNormal(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            var matrix = Matrix4x4.TRS(Center, Orientation, Size).inverse;
            var surfacePosition = matrix.MultiplyPoint3x4(GetAverageSurfacePosition(collider));
            var absSurfacePosition = new Vector3(Mathf.Abs(surfacePosition.x), Mathf.Abs(surfacePosition.y), Mathf.Abs(surfacePosition.z));
            float max = Mathf.Max(absSurfacePosition.x, absSurfacePosition.y, absSurfacePosition.z);
            Vector3 normal;

            if (max == absSurfacePosition.x)
                normal = new Vector3(Mathf.Sign(surfacePosition.x), 0, 0);
            else if (max == absSurfacePosition.y)
                normal = new Vector3(0, Mathf.Sign(surfacePosition.y), 0);
            else
                normal = new Vector3(0, 0, Mathf.Sign(surfacePosition.z));

            return matrix.inverse.MultiplyPoint3x4(normal).normalized;
        }

        protected override void Sweep()
        {
            if ((gizmoMode & GizmoMode.Activity) != 0)
                activityDrawTick = true;

            recentHits.Clear();

            Vector3 halfSize = Size * 0.5f;
            Vector3 forward = Orientation * Vector3.forward;
            Vector3 back = Orientation * Vector3.back;
            Vector3 left = Orientation * Vector3.left;
            Vector3 right = Orientation * Vector3.right;
            Vector3 up = Orientation * Vector3.up;
            Vector3 down = Orientation * Vector3.down;

            Vector3 backStart = Center + (back * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
            Vector3 backEnd = backStart + (down * Size.y);

            Vector3 frontStart = Center + (forward * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
            Vector3 frontEnd = frontStart + (down * Size.y);

            Vector3 leftStart = Center + (left * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
            Vector3 leftEnd = leftStart + (down * Size.y);

            Vector3 rightStart = Center + (right * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
            Vector3 rightEnd = rightStart + (down * Size.y);

            Vector3 topStart = Center + (up * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
            Vector3 topEnd = topStart + (back * Size.z);

            Vector3 bottomStart = Center + (down * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
            Vector3 bottomEnd = bottomStart + (back * Size.z);

            ArrayCast(backStart, backEnd, right, Size.x, forward, Size.z);
            ArrayCast(frontStart, frontEnd, right, Size.x, back, Size.z);
            ArrayCast(leftStart, leftEnd, back, Size.z, right, Size.x);
            ArrayCast(rightStart, rightEnd, back, Size.z, left, Size.x);
            ArrayCast(topStart, topEnd, right, Size.x, down, Size.y);
            ArrayCast(bottomStart, bottomEnd, right, Size.x, up, Size.y);
        }

        private void ArrayCast(Vector3 start, Vector3 end, Vector3 columnOffsetDirection, float columnOffsetDistance, Vector3 direction, float distance)
        {
            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tY = (float)row / (raysPerFace - 1);
                    float tX = (float)column / (raysPerFace - 1);

                    Vector3 origin = Vector3.Lerp(start, end, tY) + (columnOffsetDistance * tX * columnOffsetDirection);

                    int hits = Physics.RaycastNonAlloc(origin, direction, results, distance, collisionMask, QueryTriggerInteraction.Collide);

                    if (hits > 0)
                        AddValidHits(hits);
                }
            }
        }

        private void AddValidHits(int hits)
        {
            for (int i = 0; i < hits; i++)
            {
                RaycastHit result = results[i];

                if (result.collider == null)
                    continue;

                if (recentHits.TryGetValue(result.collider, out var recentHitsForCollider))
                    recentHitsForCollider.Add(result);
                else
                    recentHits.Add(result.collider, new List<RaycastHit> { result });

                if (finalHits.TryGetValue(result.collider, out var oldHit))
                {
                    if (oldHit.distance < result.distance)
                        finalHits[result.collider] = result;
                }
                else
                    finalHits.Add(result.collider, result);

                if ((gizmoMode & GizmoMode.Hits) != 0)
                    Debug.DrawLine(Center, result.point, Color.red);
            }
        }

        private void OnDrawGizmos()
        {
            if ((gizmoMode & GizmoMode.Hitbox) != 0)
            {
                var originalMatrix = Gizmos.matrix;
                var newMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                newMatrix *= Matrix4x4.TRS(center, Quaternion.Euler(orientation), Vector3.one);

                Gizmos.matrix = newMatrix;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Vector3.zero, size);
                Gizmos.matrix = originalMatrix;
            }

            if ((gizmoMode & GizmoMode.Rays) != 0)
            {
                Vector3 halfSize = Size * 0.5f;
                Vector3 forward = Orientation * Vector3.forward;
                Vector3 back = Orientation * Vector3.back;
                Vector3 left = Orientation * Vector3.left;
                Vector3 right = Orientation * Vector3.right;
                Vector3 up = Orientation * Vector3.up;
                Vector3 down = Orientation * Vector3.down;

                Vector3 backStart = Center + (back * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 backEnd = backStart + (down * Size.y);

                Vector3 frontStart = Center + (forward * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 frontEnd = frontStart + (down * Size.y);

                Vector3 leftStart = Center + (left * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 leftEnd = leftStart + (down * Size.y);

                Vector3 rightStart = Center + (right * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 rightEnd = rightStart + (down * Size.y);

                Vector3 topStart = Center + (up * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 topEnd = topStart + (back * Size.z);

                Vector3 bottomStart = Center + (down * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 bottomEnd = bottomStart + (back * Size.z);

                DrawArrayCast(backStart, backEnd, right, Size.x, forward, Size.z);
                DrawArrayCast(frontStart, frontEnd, right, Size.x, back, Size.z);
                DrawArrayCast(leftStart, leftEnd, back, Size.z, right, Size.x);
                DrawArrayCast(rightStart, rightEnd, back, Size.z, left, Size.x);
                DrawArrayCast(topStart, topEnd, right, Size.x, down, Size.y);
                DrawArrayCast(bottomStart, bottomEnd, right, Size.x, up, Size.y);
            }

            if ((gizmoMode & GizmoMode.Activity) != 0 && activityDrawTick)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Center, Size);

                activityDrawTick = false;
            }

            if ((gizmoMode & GizmoMode.HitAverages) != 0)
            {
                const float radius = 0.05f;

                foreach (var collider in recentHits.Keys)
                {
                    var averagePosition = GetAverageHitPosition(collider);
                    var averageSurfacePosition = GetAverageSurfacePosition(collider);
                    var averageNormal = GetAverageSurfaceNormal(collider);
                    var normalOrigin = averageSurfacePosition + (radius * averageNormal);

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(averagePosition, radius);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(averageSurfacePosition, radius);

                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(normalOrigin, 2f * radius * averageNormal);
                }
            }
        }

        private void DrawArrayCast(Vector3 start, Vector3 end, Vector3 columnOffsetDirection, float columnOffsetDistance, Vector3 direction, float distance)
        {
            Gizmos.color = Color.yellow;

            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tY = (float)row / (raysPerFace - 1);
                    float tX = (float)column / (raysPerFace - 1);

                    Vector3 origin = Vector3.Lerp(start, end, tY) + (columnOffsetDistance * tX * columnOffsetDirection);
                    Gizmos.DrawRay(origin, direction * distance);
                }
            }
        }
    }
}