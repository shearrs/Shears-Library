using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox2D : HitBody2D
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
            Right = 1 << 3
        }

        [Serializable]
        private struct GizmoSettings
        {
            [Header("Modes")]
            [SerializeField] private GizmoModes gizmoModes;

            [Header("Colors")]
            [SerializeField] private Color hitboxColor;
            [SerializeField] private Color rayColor;
            [SerializeField] private Color activityColor;
            [SerializeField] private Color averageHitColor;
            [SerializeField] private Color averageSurfaceColor;
            [SerializeField] private Color averageNormalColor;

            public GizmoModes Modes { readonly get => gizmoModes; set => gizmoModes = value; }
            public Color HitboxColor { readonly get => hitboxColor; set => hitboxColor = value; }
            public Color RayColor { readonly get => rayColor; set => rayColor = value; }
            public Color ActivityColor { readonly get => activityColor; set => activityColor = value; }
            public Color AverageHitColor { readonly get => averageHitColor; set => averageHitColor = value; }
            public Color AverageSurfaceColor { readonly get => averageSurfaceColor; set => averageSurfaceColor = value; }
            public Color AverageNormalColor { readonly get => averageNormalColor; set => averageNormalColor = value; }
        }

        private readonly struct ArrayCastData
        {
            private readonly Vector2 start;
            private readonly Vector2 end;
            private readonly Vector2 direction;
            private readonly float distance;

            public readonly Vector2 Start => start;
            public readonly Vector2 End => end;
            public readonly Vector2 Direction => direction;
            public readonly float Distance => distance;

            public ArrayCastData(Vector2 start, Vector2 end, Vector2 direction, float distance)
            {
                this.start = start;
                this.end = end;
                this.direction = direction;
                this.distance = distance;
            }
        }
        #endregion

        [Header("Gizmos")]
        [SerializeField] private GizmoSettings gizmoSettings;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerSide = 8;
        [SerializeField] private SourceDirections sourceDirections = (SourceDirections)(-1);

        [Header("Transform Settings")]
        [SerializeField] private Vector2 offset;
        [SerializeField, Range(0, 360)] private float angle = 0f;
        [SerializeField] private Vector2 size = Vector2.one;

        private readonly Dictionary<SourceDirections, List<HitRay2D>> directionalHitRays = new();
        private readonly Dictionary<Collider2D, List<RaycastHit2D>> recentHits = new();

        public Vector2 Center => transform.TransformPoint(offset);
        public Quaternion Orientation => transform.rotation * Quaternion.Euler(new(0, 0, angle));
        public Vector2 Size => size * transform.lossyScale;

        private void Reset()
        {
            ResetGizmoSettings();
        }

        protected override void Awake()
        {
            base.Awake();

            List<HitRay2D> getHitRays()
            {
                var list = new List<HitRay2D>();

                for (int i = 0; i < raysPerSide; i++)
                    list.Add(new(32));

                return list;
            }

            directionalHitRays.Add(SourceDirections.Top, getHitRays());
            directionalHitRays.Add(SourceDirections.Bottom, getHitRays());
            directionalHitRays.Add(SourceDirections.Left, getHitRays());
            directionalHitRays.Add(SourceDirections.Right, getHitRays());

            foreach (var hitRayList in directionalHitRays.Values)
                hitRays.AddRange(hitRayList);
        }

        #region Average Getters
        public Vector2 GetAverageHitPosition(Collider2D collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector2.zero;

            Vector2 averageHitPosition = Vector2.zero;

            foreach (var hit in recentHits[collider])
                averageHitPosition += hit.point;

            averageHitPosition /= recentHits[collider].Count;

            return averageHitPosition;
        }

        public Vector2 GetAverageSurfacePosition(Collider2D collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector2.zero;

            Vector3 size = Size;
            size.z = 1;
            var noScaleMatrix = Matrix4x4.TRS(Center, Orientation, Vector3.one).inverse;
            var fullMatrix = Matrix4x4.TRS(Center, Orientation, size).inverse;

            Vector2 averagePosition = GetAverageHitPosition(collider);

            Vector2 positionDistances = noScaleMatrix.MultiplyPoint3x4(averagePosition);
            positionDistances = new Vector2(Mathf.Abs(positionDistances.x), Mathf.Abs(positionDistances.y));
            positionDistances = new((size.x * 0.5f) - positionDistances.x, (size.y * 0.5f) - positionDistances.y);

            Vector2 localAveragePosition = (Vector2)fullMatrix.MultiplyPoint3x4(averagePosition);
            Vector2 localAbsAveragePosition = new(Mathf.Abs(localAveragePosition.x), Mathf.Abs(localAveragePosition.y));
            Vector2 surfacePosition;

            if (positionDistances.x < positionDistances.y)
                surfacePosition = localAveragePosition + (Vector2.right * ((.5f - localAbsAveragePosition.x) * Mathf.Sign(localAveragePosition.x)));
            else
                surfacePosition = localAveragePosition + (Vector2.up * ((.5f - localAbsAveragePosition.y) * Mathf.Sign(localAveragePosition.y)));

            return fullMatrix.inverse.MultiplyPoint3x4(surfacePosition);
        }

        public Vector2 GetAverageSurfaceNormal(Collider2D collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector2.zero;

            Vector3 size = Size;
            size.z = 1;
            var matrix = Matrix4x4.TRS(Center, Orientation, size).inverse;
            var surfacePosition = matrix.MultiplyPoint3x4(GetAverageSurfacePosition(collider));
            Vector2 normal;

            if (Mathf.Abs(surfacePosition.x) > Mathf.Abs(surfacePosition.y))
                normal = new Vector2(Mathf.Sign(surfacePosition.x), 0);
            else
                normal = new Vector2(0, Mathf.Sign(surfacePosition.y));

            return matrix.inverse.MultiplyPoint3x4(normal).normalized;
        }
        #endregion

        protected override void Sweep()
        {
            if ((gizmoSettings.Modes & GizmoModes.Activity) != 0)
                activityDrawTick = true;

            recentHits.Clear();

            if ((sourceDirections & SourceDirections.Left) != 0)
                ArrayCast(SourceDirections.Left);

            if ((sourceDirections & SourceDirections.Right) != 0)
                ArrayCast(SourceDirections.Right);

            if ((sourceDirections & SourceDirections.Bottom) != 0)
                ArrayCast(SourceDirections.Bottom);

            if ((sourceDirections & SourceDirections.Top) != 0)
                ArrayCast(SourceDirections.Top);
        }

        private void ArrayCast(SourceDirections rayDirection)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = collisionMask,
            };

            var castData = GetArrayCastData(rayDirection);

            for (int i = 0; i < directionalHitRays[rayDirection].Count; i++)
            {
                var ray = directionalHitRays[rayDirection][i];
                float t = (float)i / (raysPerSide - 1);
                Vector2 origin = Vector2.Lerp(castData.Start, castData.End, t);

                ray.ClearValidHits();
                ray.Cast(origin, castData.Direction, filter, castData.Distance);

                AddValidHits(ray);
            }
        }

        private ArrayCastData GetArrayCastData(SourceDirections direction)
        {
            Vector2 halfSize = Size * 0.5f;
            Vector2 left = Orientation * Vector2.left;
            Vector2 right = Orientation * Vector2.right;
            Vector2 up = Orientation * Vector2.up;
            Vector2 down = Orientation * Vector2.down;

            switch (direction)
            {
                case SourceDirections.Top:
                    Vector2 topStart = Center + (left * halfSize.x) + (up * halfSize.y);
                    Vector2 topEnd = topStart + (right * Size.x);
                    return new(topStart, topEnd, down, Size.y);
                case SourceDirections.Bottom:
                    Vector2 bottomStart = Center + (left * halfSize.x) + (down * halfSize.y);
                    Vector2 bottomEnd = bottomStart + (right * Size.x);
                    return new(bottomStart, bottomEnd, up, Size.y);
                case SourceDirections.Left:
                    Vector2 leftStart = Center + (left * halfSize.x) + (up * halfSize.y);
                    Vector2 leftEnd = leftStart + (down * Size.y);
                    return new(leftStart, leftEnd, right, Size.x);
                case SourceDirections.Right:
                    Vector2 rightStart = Center + (right * halfSize.x) + (up * halfSize.y);
                    Vector2 rightEnd = rightStart + (down * Size.y);
                    return new(rightStart, rightEnd, left, Size.x);
            }

            return default;
        }

        private void AddValidHits(HitRay2D ray)
        {
            for (int i = 0; i < ray.Hits; i++)
            {
                var result = ray.Results[i];

                if (result.collider == null)
                    continue;

                if (recentHits.TryGetValue(result.collider, out var recentHitsForCollider))
                    recentHitsForCollider.Add(result);
                else
                    recentHits.Add(result.collider, new List<RaycastHit2D> { result });

                if (ray.ValidHits.TryGetValue(result.collider, out var oldHit))
                {
                    if (oldHit.distance < result.distance)
                        ray.SetValidHit(result.collider, result);
                }
                else
                    ray.AddValidHit(result);

                if ((gizmoSettings.Modes & GizmoModes.Hits) != 0)
                    Debug.DrawLine(Center, result.point, Color.red);
            }
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            var originalMatrix = Gizmos.matrix;
            var newMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            newMatrix *= Matrix4x4.TRS(offset, Quaternion.Euler(0, 0, angle), Vector3.one);

            Gizmos.matrix = newMatrix;

            if ((gizmoSettings.Modes & GizmoModes.Hitbox) != 0)
            {
                Gizmos.color = gizmoSettings.HitboxColor;
                Gizmos.DrawWireCube(Vector3.zero, size);
            }

            Vector3 Center = offset;
            Vector2 Size = size;
            Quaternion Orientation = Quaternion.Euler(new(0, 0, angle));

            if ((gizmoSettings.Modes & GizmoModes.Rays) != 0)
            {
                Vector3 halfSize = Size * 0.5f;
                Vector3 left = Orientation * Vector3.left;
                Vector3 right = Orientation * Vector3.right;
                Vector3 up = Orientation * Vector3.up;
                Vector3 down = Orientation * Vector3.down;

                Vector3 leftStart = Center + (left * halfSize.x) + (up * halfSize.y);
                Vector3 leftEnd = leftStart + (down * Size.y);

                Vector3 rightStart = Center + (right * halfSize.x) + (up * halfSize.y);
                Vector3 rightEnd = rightStart + (down * Size.y);

                Vector3 bottomStart = Center + (left * halfSize.x) + (down * halfSize.y);
                Vector3 bottomEnd = bottomStart + (right * Size.x);

                Vector3 topStart = Center + (left * halfSize.x) + (up * halfSize.y);
                Vector3 topEnd = topStart + (right * Size.x);

                if ((sourceDirections & SourceDirections.Left) != 0)
                    DrawArrayCast(leftStart, leftEnd, right, Size.x);

                if ((sourceDirections & SourceDirections.Right) != 0)
                    DrawArrayCast(rightStart, rightEnd, left, Size.x);

                if ((sourceDirections & SourceDirections.Bottom) != 0)
                    DrawArrayCast(bottomStart, bottomEnd, up, Size.y);

                if ((sourceDirections & SourceDirections.Top) != 0)
                    DrawArrayCast(topStart, topEnd, down, Size.y);
            }

            if ((gizmoSettings.Modes & GizmoModes.Activity) != 0 && activityDrawTick)
            {
                Gizmos.color = gizmoSettings.ActivityColor;
                Gizmos.DrawCube(Center, Size);

                activityDrawTick = false;
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

                    Gizmos.color = gizmoSettings.AverageHitColor;
                    Gizmos.DrawWireSphere(averagePosition, radius);

                    Gizmos.color = gizmoSettings.AverageSurfaceColor;
                    Gizmos.DrawWireSphere(averageSurfacePosition, radius);

                    Gizmos.color = gizmoSettings.AverageNormalColor;
                    Gizmos.DrawRay(normalOrigin, 2f * radius * averageNormal);
                }
            }

            Gizmos.matrix = originalMatrix;
        }

        private void DrawArrayCast(Vector2 start, Vector2 end, Vector2 direction, float distance)
        {
            Gizmos.color = gizmoSettings.RayColor;

            for (int i = 0; i < raysPerSide; i++)
            {
                float t = (float)i / (raysPerSide - 1);
                Vector2 origin = Vector2.Lerp(start, end, t);

                GizmoUtil.DrawArrow(origin, direction * distance, Vector2.Perpendicular(direction), 0.075f, 0.075f, Color.magenta);
                //Gizmos.DrawRay(origin, direction * distance);
            }
        }

        public void ResetGizmoSettings()
        {
            gizmoSettings.Modes = GizmoModes.Hitbox | GizmoModes.Rays | GizmoModes.Hits;

            gizmoSettings.HitboxColor = Color.red;
            gizmoSettings.RayColor = Color.yellow;
            gizmoSettings.ActivityColor = Color.red;
            gizmoSettings.AverageHitColor = Color.magenta;
            gizmoSettings.AverageSurfaceColor = Color.green;
            gizmoSettings.AverageNormalColor = Color.cyan;
        }
        #endregion
    }
}
