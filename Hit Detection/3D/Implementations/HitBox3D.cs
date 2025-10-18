using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox3D : HitBody3D
    {
        #region Nested Types
        [Flags]
        private enum GizmoModes
        {
            Hitbox = 1 << 0,
            Rays = 1 << 1,
            Hits = 1 << 2,
            HitAverages = 1 << 3,
            Activity = 1 << 4
        }

        [Flags]
        private enum SourceDirections
        {
            Top = 1 << 0,
            Bottom = 1 << 1,
            Left = 1 << 2,
            Right = 1 << 3,
            Front = 1 << 4,
            Back = 1 << 5
        }

        [Serializable]
        private struct GizmoSettings
        {
            [Header("Modes")]
            [SerializeField] private GizmoModes gizmoModes;

            [Header("Colors")]
            [SerializeField] private Color hitboxColor;
            [SerializeField] private Color rayColor;
            [SerializeField] private Color averageHitColor;
            [SerializeField] private Color averageSurfaceColor;
            [SerializeField] private Color averageNormalColor;

            public GizmoModes Modes { readonly get => gizmoModes; set => gizmoModes = value; }
            public Color HitboxColor { readonly get => hitboxColor; set => hitboxColor = value; }
            public Color RayColor { readonly get => rayColor; set => rayColor = value; }
            public Color AverageHitColor { readonly get => averageHitColor; set => averageHitColor = value; }
            public Color AverageSurfaceColor { readonly get => averageSurfaceColor; set => averageSurfaceColor = value; }
            public Color AverageNormalColor { readonly get => averageNormalColor; set => averageNormalColor = value; }
        }
        #endregion

        [Header("Gizmos")]
        [SerializeField] private GizmoSettings gizmoSettings;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerFace = 3;
        [SerializeField] private SourceDirections sourceDirections = (SourceDirections)(-1);

        [Header("Transform Settings")]
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 orientation = Vector3.zero;
        [SerializeField] private Vector3 size = Vector3.one;

        private readonly Dictionary<Collider, List<RaycastHit>> recentHits = new();
        private readonly RaycastHit[] results = new RaycastHit[100];

        private Vector3 TCenter => transform.position + center;
        private Quaternion TOrientation => transform.rotation * Quaternion.Euler(orientation);
        private Vector3 TSize => Vector3.Scale(size, transform.lossyScale);

        public Vector3 Center { get => center; set => center = value; }
        public Quaternion Orientation { get => Quaternion.Euler(orientation); set => orientation = value.eulerAngles; }
        public Vector3 Size { get => size; set => size = value; }

        private void Reset()
        {
            ResetGizmoSettings();
        }

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

        public Vector3 GetAverageHitNormal(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            Vector3 averageHitNormal = Vector3.zero;

            foreach (var hit in recentHits[collider])
                averageHitNormal += hit.normal;

            averageHitNormal /= recentHits[collider].Count;

            return averageHitNormal;
        }

        public Vector3 GetAverageSurfacePosition(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            var noScaleMatrix = Matrix4x4.TRS(TCenter, TOrientation, Vector3.one).inverse;
            var fullMatrix = Matrix4x4.TRS(TCenter, TOrientation, TSize).inverse;

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

            var matrix = Matrix4x4.TRS(TCenter, TOrientation, TSize).inverse;
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

            return normal.normalized;
        }

        protected override void Sweep()
        {
            recentHits.Clear();

            Vector3 halfSize = TSize * 0.5f;
            Vector3 forward = TOrientation * Vector3.forward;
            Vector3 back = TOrientation * Vector3.back;
            Vector3 left = TOrientation * Vector3.left;
            Vector3 right = TOrientation * Vector3.right;
            Vector3 up = TOrientation * Vector3.up;
            Vector3 down = TOrientation * Vector3.down;

            Vector3 backStart = TCenter + (back * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
            Vector3 backEnd = backStart + (down * TSize.y);

            Vector3 frontStart = TCenter + (forward * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
            Vector3 frontEnd = frontStart + (down * TSize.y);

            Vector3 leftStart = TCenter + (left * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
            Vector3 leftEnd = leftStart + (down * TSize.y);

            Vector3 rightStart = TCenter + (right * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
            Vector3 rightEnd = rightStart + (down * TSize.y);

            Vector3 topStart = TCenter + (up * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
            Vector3 topEnd = topStart + (back * TSize.z);

            Vector3 bottomStart = TCenter + (down * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
            Vector3 bottomEnd = bottomStart + (back * TSize.z);

            if ((sourceDirections & SourceDirections.Back) != 0)
                ArrayCast(backStart, backEnd, right, TSize.x, forward, TSize.z);

            if ((sourceDirections & SourceDirections.Front) != 0)
                ArrayCast(frontStart, frontEnd, right, TSize.x, back, TSize.z);

            if ((sourceDirections & SourceDirections.Left) != 0)
                ArrayCast(leftStart, leftEnd, back, TSize.z, right, TSize.x);

            if ((sourceDirections & SourceDirections.Right) != 0)
                ArrayCast(rightStart, rightEnd, back, TSize.z, left, TSize.x);

            if ((sourceDirections & SourceDirections.Top) != 0)
                ArrayCast(topStart, topEnd, right, TSize.x, down, TSize.y);

            if ((sourceDirections & SourceDirections.Bottom) != 0)
                ArrayCast(bottomStart, bottomEnd, right, TSize.x, up, TSize.y);
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

                if ((gizmoSettings.Modes & GizmoModes.Hits) != 0)
                    Debug.DrawLine(TCenter, result.point, Color.red);
            }
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            Color opacity = isActiveAndEnabled ? Color.white : new(1, 1, 1, 0.15f);

            if ((gizmoSettings.Modes & GizmoModes.Hitbox) != 0)
            {
                var originalMatrix = Gizmos.matrix;
                var newMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                newMatrix *= Matrix4x4.TRS(center, Quaternion.Euler(orientation), Vector3.one);

                Gizmos.matrix = newMatrix;
                Gizmos.color = Color.red * opacity;
                Gizmos.DrawWireCube(Vector3.zero, size);
                Gizmos.matrix = originalMatrix;
            }

            if ((gizmoSettings.Modes & GizmoModes.Rays) != 0)
            {
                Vector3 halfSize = TSize * 0.5f;
                Vector3 forward = TOrientation * Vector3.forward;
                Vector3 back = TOrientation * Vector3.back;
                Vector3 left = TOrientation * Vector3.left;
                Vector3 right = TOrientation * Vector3.right;
                Vector3 up = TOrientation * Vector3.up;
                Vector3 down = TOrientation * Vector3.down;

                Vector3 backStart = TCenter + (back * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 backEnd = backStart + (down * TSize.y);

                Vector3 frontStart = TCenter + (forward * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 frontEnd = frontStart + (down * TSize.y);

                Vector3 leftStart = TCenter + (left * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 leftEnd = leftStart + (down * TSize.y);

                Vector3 rightStart = TCenter + (right * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 rightEnd = rightStart + (down * TSize.y);

                Vector3 topStart = TCenter + (up * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 topEnd = topStart + (back * TSize.z);

                Vector3 bottomStart = TCenter + (down * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 bottomEnd = bottomStart + (back * TSize.z);

                if ((sourceDirections & SourceDirections.Back) != 0)
                    DrawArrayCast(backStart, backEnd, right, TSize.x, forward, TSize.z);

                if ((sourceDirections & SourceDirections.Front) != 0)
                    DrawArrayCast(frontStart, frontEnd, right, TSize.x, back, TSize.z);

                if ((sourceDirections & SourceDirections.Left) != 0)
                    DrawArrayCast(leftStart, leftEnd, back, TSize.z, right, TSize.x);

                if ((sourceDirections & SourceDirections.Right) != 0)
                    DrawArrayCast(rightStart, rightEnd, back, TSize.z, left, TSize.x);

                if ((sourceDirections & SourceDirections.Top) != 0)
                    DrawArrayCast(topStart, topEnd, right, TSize.x, down, TSize.y);

                if ((sourceDirections & SourceDirections.Bottom) != 0)
                    DrawArrayCast(bottomStart, bottomEnd, right, TSize.x, up, TSize.y);
            }

            if ((gizmoSettings.Modes & GizmoModes.HitAverages) != 0)
            {
                const float radius = 0.05f;

                foreach (var collider in recentHits.Keys)
                {
                    var averagePosition = GetAverageHitPosition(collider);
                    var averageSurfacePosition = GetAverageSurfacePosition(collider);
                    var averageNormal = GetAverageSurfaceNormal(collider);
                    var normalOrigin = averageSurfacePosition + (radius * averageNormal);

                    Gizmos.color = gizmoSettings.AverageHitColor * opacity;
                    Gizmos.DrawWireSphere(averagePosition, radius);

                    Gizmos.color = gizmoSettings.AverageSurfaceColor * opacity;
                    Gizmos.DrawWireSphere(averageSurfacePosition, radius);

                    Gizmos.color = gizmoSettings.AverageNormalColor * opacity;
                    Gizmos.DrawRay(normalOrigin, 2f * radius * averageNormal);
                }
            }
        }

        private void DrawArrayCast(Vector3 start, Vector3 end, Vector3 columnOffsetDirection, float columnOffsetDistance, Vector3 direction, float distance)
        {
            Color opacity = isActiveAndEnabled ? Color.white : new(1, 1, 1, 0.15f);
            Gizmos.color = Color.yellow * opacity;

            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tY = (float)row / (raysPerFace - 1);
                    float tX = (float)column / (raysPerFace - 1);

                    Vector3 origin = Vector3.Lerp(start, end, tY) + (columnOffsetDistance * tX * columnOffsetDirection);
                    Vector3 crossAgainst = (direction == Vector3.up || direction == Vector3.down) ? Vector3.forward : Vector3.up;

                    GizmosUtil.DrawArrow(origin, direction * distance, Vector3.Cross(direction, crossAgainst), 0.075f, 0.075f, Color.magenta * opacity);
                }
            }
        }

        public void ResetGizmoSettings()
        {
            gizmoSettings.Modes = GizmoModes.Hitbox | GizmoModes.Rays | GizmoModes.Hits;

            gizmoSettings.HitboxColor = Color.red;
            gizmoSettings.RayColor = Color.yellow;
            gizmoSettings.AverageHitColor = Color.magenta;
            gizmoSettings.AverageSurfaceColor = Color.green;
            gizmoSettings.AverageNormalColor = Color.cyan;
        }
        #endregion
    }
}