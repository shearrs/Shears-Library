using System;
using System.Buffers;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Shears.HitDetection
{
    public class HitSphere : HitShape3D
    {
        private const float EDGE_OFFSET = 0.1f;

        #region Nested Types
        [Flags]
        private enum GizmoModes
        {
            Sphere = 1 << 0,
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
            [SerializeField] private Color sphereColor;
            [SerializeField] private Color rayColor;

            public GizmoModes Modes { readonly get => gizmoModes; set => gizmoModes = value; }
            public bool DrawOnSelect { readonly get => drawOnSelect; set => drawOnSelect = value; }
            public Color SphereColor { readonly get => sphereColor; set => sphereColor = value; }
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
        private int raysPerSide = 5;

        [SerializeField]
        private SourceDirections sourceDirections = (SourceDirections)(-1);

        [Header("Transform Settings")]
        [SerializeField]
        private Vector3 center;

        [SerializeField]
        private Vector3 orientation = Vector3.zero;

        [SerializeField]
        private float radius = 0.5f;

        private RaycastHit[] results;
        private bool isDetecting = false;

        private Vector3 TCenter => transform.position + center;
        private float TRadius => transform.localScale.GetAverage() * radius;

        public Vector3 Center { get => center; set => center = value; }
        public Quaternion Orientation { get => Quaternion.Euler(orientation); set => orientation = value.eulerAngles; }
        public float Radius { get => radius; set => radius = value; }

        public Vector3 WorldCenter { get => transform.TransformPoint(center); set => center = transform.InverseTransformPoint(value); }
        #endregion

        private void Reset()
        {
            ResetGizmoSettings();
        }

        private void Awake()
        {
            results = ArrayPool<RaycastHit>.Shared.Rent(maxHits);
        }

        private void OnDestroy()
        {
            ArrayPool<RaycastHit>.Shared.Return(results);
        }

        internal override void Sweep(DetectionHandle handle)
        {
            isDetecting = true;

            var orientation = Quaternion.Euler(this.orientation);
            Vector3 forward = orientation * transform.TransformDirection(Vector3.forward);
            Vector3 back = orientation * transform.TransformDirection(Vector3.back);
            Vector3 right = orientation * transform.TransformDirection(Vector3.right);
            Vector3 left = orientation * transform.TransformDirection(Vector3.left);
            Vector3 up = orientation * transform.TransformDirection(Vector3.up);
            Vector3 down = orientation * transform.TransformDirection(Vector3.down);

            if ((sourceDirections & SourceDirections.Back) != 0)
                ArrayCast(forward, right, up, handle);

            if ((sourceDirections & SourceDirections.Front) != 0)
                ArrayCast(back, left, up, handle);

            if ((sourceDirections & SourceDirections.Left) != 0)
                ArrayCast(right, back, up, handle);

            if ((sourceDirections & SourceDirections.Right) != 0)
                ArrayCast(left, forward, up, handle);

            if ((sourceDirections & SourceDirections.Top) != 0)
                ArrayCast(down, right, forward, handle);

            if ((sourceDirections & SourceDirections.Bottom) != 0)
                ArrayCast(up, right, back, handle);
        }

        private void ArrayCast(
            Vector3 direction, Vector3 offsetDirectionX, 
            Vector3 offsetDirectionY, DetectionHandle handle
        )
        {
            float trueRadius = TRadius;
            float radius = trueRadius - EDGE_OFFSET;
            float diameter = 2.0f * radius;
            Vector3 startOffset = -radius * (offsetDirectionX + offsetDirectionY);

            for (int column = 0; column < raysPerSide; column++)
            {
                for (int row = 0; row < raysPerSide; row++)
                {
                    float tX = (float)row / (raysPerSide - 1);
                    float tY = (float)column / (raysPerSide - 1);

                    Vector3 offset = startOffset + diameter * ((tX * offsetDirectionX) + (tY * offsetDirectionY));
                    float distance = Mathf.Sqrt(trueRadius * trueRadius - offset.sqrMagnitude);

                    Vector3 origin = TCenter + offset + (distance * -direction);
                    distance *= 2.0f;

                    int hits = Physics.RaycastNonAlloc(origin, direction, results, distance, handle.CollisionMask, QueryTriggerInteraction.Collide);

                    if (hits > 0)
                        handle.ValidateCallback(results, hits, null, out _);
                }
            }
        }

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (!gizmoSettings.DrawOnSelect)
                DrawSphereGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (gizmoSettings.DrawOnSelect)
                DrawSphereGizmos();
        }

        private void DrawSphereGizmos()
        {
            Color opacity = isDetecting ? Color.white : new(1, 1, 1, 0.15f);

            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(orientation), Vector3.one);

            if ((gizmoSettings.Modes & GizmoModes.Sphere) != 0)
            {
                Gizmos.color = opacity * Color.red;
                Gizmos.DrawWireSphere(center, radius);
            }

            if ((gizmoSettings.Modes & GizmoModes.Rays) != 0)
            {
                Gizmos.color = opacity * Color.yellow;

                if ((sourceDirections & SourceDirections.Back) != 0)
                    DrawArrayCast(Vector3.forward, Vector3.right, Vector3.up);

                if ((sourceDirections & SourceDirections.Front) != 0)
                    DrawArrayCast(Vector3.back, Vector3.left, Vector3.up);

                if ((sourceDirections & SourceDirections.Left) != 0)
                    DrawArrayCast(Vector3.right, Vector3.back, Vector3.up);

                if ((sourceDirections & SourceDirections.Right) != 0)
                    DrawArrayCast(Vector3.left, Vector3.forward, Vector3.up);

                if ((sourceDirections & SourceDirections.Top) != 0)
                    DrawArrayCast(Vector3.down, Vector3.right, Vector3.forward);

                if ((sourceDirections & SourceDirections.Bottom) != 0)
                    DrawArrayCast(Vector3.up, Vector3.right, Vector3.back);
            }

            Gizmos.matrix = matrix;

            if (isDetecting)
                isDetecting = false;
        }

        private void DrawArrayCast(Vector3 direction, Vector3 offsetDirectionX, Vector3 offsetDirectionY)
        {
            float radius = this.radius - EDGE_OFFSET;
            float diameter = 2.0f * radius;
            Vector3 startOffset = -radius * (offsetDirectionX + offsetDirectionY);

            for (int column = 0; column < raysPerSide; column++)
            {
                for (int row = 0; row < raysPerSide; row++)
                {
                    float tX = (float)row / (raysPerSide - 1);
                    float tY = (float)column / (raysPerSide - 1);
                    
                    Vector3 offset = startOffset + diameter * ((tX * offsetDirectionX) + (tY * offsetDirectionY));
                    float distance = Mathf.Sqrt(this.radius * this.radius - offset.sqrMagnitude);

                    Vector3 origin = center + offset + (distance * -direction);
                    distance *= 2.0f;

                    Gizmos.DrawRay(origin, distance * direction);
                }
            }
        }

        [ContextMenu("Reset Gizmo Settings")]
        public void ResetGizmoSettings()
        {
            gizmoSettings.Modes = GizmoModes.Sphere | GizmoModes.Rays;
            gizmoSettings.DrawOnSelect = true;

            gizmoSettings.SphereColor = Color.red;
            gizmoSettings.RayColor = Color.yellow;
        }
        #endregion
    }
}
