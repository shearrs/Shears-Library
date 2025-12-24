using Shears;
using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Shears.HitDetection
{
    public class HitBox3D : HitShape3D
    {
        #region Nested Types
        [Flags]
        private enum GizmoModes
        {
            Hitbox = 1 << 0,
            Rays = 1 << 1,
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
            [SerializeField] private bool drawOnSelect;

            [Header("Colors")]
            [SerializeField] private Color hitboxColor;
            [SerializeField] private Color rayColor;

            public GizmoModes Modes { readonly get => gizmoModes; set => gizmoModes = value; }
            public bool DrawOnSelect { readonly get => drawOnSelect; set => drawOnSelect = value; }
            public Color HitboxColor { readonly get => hitboxColor; set => hitboxColor = value; }
            public Color RayColor { readonly get => rayColor; set => rayColor = value; }
        }
        #endregion

        #region Variables
        [Header("Gizmos")]
        [SerializeField]
        private GizmoSettings gizmoSettings;

        [Header("Collision Settings")]
        [SerializeField, Range(0, 500), RuntimeReadonly]
        private int maxHits = 10;

        [SerializeField, Range(2, 32), RuntimeReadonly]
        private int raysPerFace = 3;

        [SerializeField]
        private SourceDirections sourceDirections = (SourceDirections)(-1);

        [SerializeField]
        private bool continuousDetection = false;

        [SerializeField, Min(0.01f), ShowIf("continuousDetection")]
        private float continuousDetectionStep = 0.25f;

        [Header("Transform Settings")]
        [SerializeField]
        private Vector3 center;

        [SerializeField]
        private Vector3 orientation = Vector3.zero;

        [SerializeField]
        private Vector3 size = Vector3.one;

        private readonly HashSet<HitRay> blockedRays = new();
        private RaycastHit[] results;
        private bool isDetecting = false;
        private Vector3 previousPosition;

        private Vector3 TCenter => transform.position + TOrientation * center;
        private Quaternion TOrientation => transform.rotation * Quaternion.Euler(orientation);
        private Vector3 TSize => Vector3.Scale(size, transform.lossyScale);

        public Vector3 Center { get => center; set => center = value; }
        public Quaternion Orientation { get => Quaternion.Euler(orientation); set => orientation = value.eulerAngles; }
        public Vector3 Size { get => size; set => size = value; }

        public Vector3 WorldCenter { get => transform.TransformPoint(center); set => center = transform.InverseTransformPoint(value); }
        #endregion

        private readonly struct HitRay
        {
            private readonly SourceDirections direction;
            private readonly int row;
            private readonly int column;

            public readonly SourceDirections Direction => direction;
            public readonly int Row => row;
            public readonly int Column => column;

            public HitRay(SourceDirections direction, int row, int column)
            {
                this.direction = direction;
                this.row = row;
                this.column = column;
            }
        }

        private void Reset()
        {
            ResetGizmoSettings();
        }

        private void Awake()
        {
            results = ArrayPool<RaycastHit>.Shared.Rent(maxHits);
        }

        private void Update()
        {
            previousPosition = TCenter;
        }

        private void OnDestroy()
        {
            ArrayPool<RaycastHit>.Shared.Return(results);
        }

        internal override void Sweep(DetectionHandle handle)
        {
            isDetecting = true;
            blockedRays.Clear();

            ArrayCastDirections(TCenter, handle);

            if (continuousDetection)
                ContinuousSweep(handle);
        }

        private void ContinuousSweep(DetectionHandle handle)
        {
            Vector3 heading = TCenter - previousPosition;
            float distance = heading.magnitude;

            if (distance < continuousDetectionStep)
                return;

            Vector3 direction = heading / distance;
            float traveled = 0f;

            while (traveled < distance)
            {
                Vector3 currentOrigin = previousPosition + direction * traveled;
                ArrayCastDirections(currentOrigin, handle);

                traveled += continuousDetectionStep;
            }
        }

        private void ArrayCastDirections(Vector3 origin, DetectionHandle handle)
        {
            Vector3 halfSize = TSize * 0.5f;
            Vector3 forward = TOrientation * Vector3.forward;
            Vector3 back = TOrientation * Vector3.back;
            Vector3 left = TOrientation * Vector3.left;
            Vector3 right = TOrientation * Vector3.right;
            Vector3 up = TOrientation * Vector3.up;
            Vector3 down = TOrientation * Vector3.down;

            if ((sourceDirections & SourceDirections.Back) != 0)
            {
                Vector3 backStart = origin + (back * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 backEnd = backStart + (down * TSize.y);
                ArrayCast(
                    SourceDirections.Back,
                    backStart, backEnd,
                    right, TSize.x,
                    forward, TSize.z,
                    handle
                );
            }

            if ((sourceDirections & SourceDirections.Front) != 0)
            {
                Vector3 frontStart = origin + (forward * halfSize.z) + (up * halfSize.y) + (left * halfSize.x);
                Vector3 frontEnd = frontStart + (down * TSize.y);

                ArrayCast(
                    SourceDirections.Front,
                    frontStart, frontEnd,
                    right, TSize.x,
                    back, TSize.z,
                    handle
                );
            }

            if ((sourceDirections & SourceDirections.Left) != 0)
            {
                Vector3 leftStart = origin + (left * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 leftEnd = leftStart + (down * TSize.y);

                ArrayCast(
                    SourceDirections.Left,
                    leftStart, leftEnd,
                    back, TSize.z,
                    right, TSize.x,
                    handle
                );
            }

            if ((sourceDirections & SourceDirections.Right) != 0)
            {
                Vector3 rightStart = origin + (right * halfSize.x) + (up * halfSize.y) + (forward * halfSize.z);
                Vector3 rightEnd = rightStart + (down * TSize.y);

                ArrayCast(
                    SourceDirections.Right,
                    rightStart, rightEnd,
                    back, TSize.z,
                    left, TSize.x,
                    handle
                );
            }

            if ((sourceDirections & SourceDirections.Top) != 0)
            {
                Vector3 topStart = origin + (up * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 topEnd = topStart + (back * TSize.z);

                ArrayCast(
                    SourceDirections.Top,
                    topStart, topEnd,
                    right, TSize.x,
                    down, TSize.y,
                    handle
                );
            }

            if ((sourceDirections & SourceDirections.Bottom) != 0)
            {
                Vector3 bottomStart = origin + (down * halfSize.y) + (left * halfSize.x) + (forward * halfSize.z);
                Vector3 bottomEnd = bottomStart + (back * TSize.z);

                ArrayCast(
                    SourceDirections.Bottom,
                    bottomStart, bottomEnd,
                    right, TSize.x,
                    up, TSize.y,
                    handle
                );
            }
        }

        private void ArrayCast(
            SourceDirections sourceDirection, 
            Vector3 start, Vector3 end,
            Vector3 columnOffsetDirection, float columnOffsetDistance,
            Vector3 direction, float distance,
            DetectionHandle handle
        )
        {
            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    var ray = new HitRay(sourceDirection, row, column);

                    if (blockedRays.Contains(ray))
                        continue;

                    float tY = (float)row / (raysPerFace - 1);
                    float tX = (float)column / (raysPerFace - 1);

                    Vector3 origin = Vector3.Lerp(start, end, tY) + (columnOffsetDistance * tX * columnOffsetDirection);

                    int hits = Physics.RaycastNonAlloc(origin, direction, results, distance, handle.CollisionMask, QueryTriggerInteraction.Collide);

                    if (hits > 0)
                    {
                        handle.ValidateCallback(results, hits, null, out bool blocked);

                        if (blocked)
                            blockedRays.Add(ray);
                    }
                }
            }
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (!gizmoSettings.DrawOnSelect)
                DrawBoxGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (gizmoSettings.DrawOnSelect)
                DrawBoxGizmos();
        }

        private void DrawBoxGizmos()
        {
            Color opacity = isDetecting ? Color.white : new(1, 1, 1, 0.15f);

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

            if (isDetecting)
                isDetecting = false;
        }

        private void DrawArrayCast(Vector3 start, Vector3 end, Vector3 columnOffsetDirection, float columnOffsetDistance, Vector3 direction, float distance)
        {
            Color opacity = isDetecting ? Color.white : new(1, 1, 1, 0.35f);
            Gizmos.color = Color.yellow * opacity;

            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tX = (float)column / (raysPerFace - 1);
                    float tY = (float)row / (raysPerFace - 1);

                    Vector3 origin = Vector3.Lerp(start, end, tY) + (columnOffsetDistance * tX * columnOffsetDirection);
                    Vector3 crossAgainst = (direction == Vector3.up || direction == Vector3.down) ? Vector3.forward : Vector3.up;

                    GizmosUtil.DrawArrow(origin, direction * distance, Vector3.Cross(direction, crossAgainst), 0.075f, 0.075f, Color.magenta * opacity);
                }
            }
        }

        [ContextMenu("Reset Gizmo Settings")]
        public void ResetGizmoSettings()
        {
            gizmoSettings.Modes = GizmoModes.Hitbox | GizmoModes.Rays;
            gizmoSettings.DrawOnSelect = true;

            gizmoSettings.HitboxColor = Color.red;
            gizmoSettings.RayColor = Color.yellow;
        }
        #endregion
    }
}