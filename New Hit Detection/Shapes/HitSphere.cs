using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitSphere : HitShape
    {
        private const float EDGE_OFFSET = 0.1f;

        [SerializeField]
        private GizmoSettings gizmoSettings = new();

        [Header("Transform")]
        [SerializeField]
        private Vector3 positionOffset;

        [SerializeField]
        private Vector3 rotationOffset;

        [SerializeField]
        private float radius = 0.5f;

        [Header("Casts")]
        [SerializeField]
        private DetectionType detectionType = DetectionType.Center;

        [SerializeField, ShowIf(nameof(detectionType), DetectionType.Center)]
        private bool isHalfSphere = false;

        [SerializeField, ShowIf(nameof(detectionType), DetectionType.Center), Range(3, 64)]
        private int centerRayCount = 8;

        [SerializeField, Range(1, 500), RuntimeReadOnly]
        private int maxHits = 10;

        [SerializeField, ShowIf(nameof(detectionType), DetectionType.Grid), Range(3, 64)]
        private int gridRayCount = 5;

        [SerializeField, ShowIf(nameof(detectionType), DetectionType.Grid)]
        private CastDirection castDirections =
            CastDirection.Up
            | CastDirection.Down
            | CastDirection.Left
            | CastDirection.Right
            | CastDirection.Forward
            | CastDirection.Back;

        private RaycastHit[] rayResults = Array.Empty<RaycastHit>();
        private Collider[] colliderResults = Array.Empty<Collider>();

        private float ScaledRadius => transform.lossyScale.GetAverage() * radius;

        [Serializable]
        private class GizmoSettings
        {
            [SerializeField]
            private Option<Color> outlineColor = new(Color.red);

            [SerializeField]
            private Option<Color> rayColor = new(Color.yellow);

            public IReadOnlyOption<Color> OutlineColor => outlineColor;
            public IReadOnlyOption<Color> RayColor => rayColor;
        }

        public enum DetectionType
        {
            Center,
            Grid,
            Overlap,
        }

        private void Awake()
        {
            CollectionUtil.GetPooled(maxHits, out rayResults);
            CollectionUtil.GetPooled(maxHits, out colliderResults);
        }

        private void OnDestroy()
        {
            if (rayResults.Length > 0)
                CollectionUtil.ReleasePooled(rayResults);

            if (colliderResults.Length > 0)
                CollectionUtil.ReleasePooled(colliderResults);
        }

        public override SweepResult Sweep(in SweepContext context)
        {
            if (detectionType == DetectionType.Center)
                return CenterSweep(context);
            else if (detectionType == DetectionType.Grid)
                return GridSweep(context);
            else
                return OverlapSweep(context);
        }

        private SweepResult CenterSweep(in SweepContext context)
        {
            float radius = ScaledRadius;
            float xStep = 360.0f / centerRayCount;
            float yStep = 360.0f / centerRayCount;
            float xDegrees = isHalfSphere ? 180.0f : 0.0f;
            float yDegrees = 0.0f;
            var rotation = Quaternion.Euler(rotationOffset);
            var center = transform.TransformPoint(positionOffset);
            var forward = rotation * transform.forward;
            int id = 0;

            CollectionUtil.GetPooled(out List<HitResult> results);
            CollectionUtil.GetPooled(out List<int> sortedHits);

            while (xDegrees < 360.0f)
            {
                while (yDegrees < 360.0f)
                {
                    var rayRotation = Quaternion.Euler(new(xDegrees, yDegrees, 0.0f));
                    var direction = rayRotation * (radius * forward);

                    int hits = Physics.RaycastNonAlloc(
                        center,
                        direction,
                        rayResults,
                        ScaledRadius,
                        context.CollisionMask,
                        QueryTriggerInteraction.Collide
                    );

                    sortedHits.Clear();

                    for (int i = 0; i < hits; i++)
                        sortedHits.Add(i);

                    sortedHits.Sort(
                        (h1, h2) => results[h1].Distance.CompareTo(results[h2].Distance)
                    );

                    foreach (var hitIndex in sortedHits)
                    {
                        var hit = rayResults[hitIndex];
                        results.Add(new(id, hit.collider, hit.point, hit.normal, hit.distance));
                    }

                    id++;
                    yDegrees += yStep;
                }

                yDegrees = 0.0f;
                xDegrees += xStep;
            }

            CollectionUtil.ReleasePooled(sortedHits);
            var finalResult = new SweepResult(results);

            return finalResult;
        }

        private SweepResult GridSweep(in SweepContext context)
        {
            var center = transform.TransformPoint(positionOffset);
            var rotation = Quaternion.Euler(rotationOffset);
            var forward = rotation * transform.forward;
            var back = rotation * -transform.forward;
            var right = rotation * transform.right;
            var left = rotation * -transform.right;
            var up = rotation * transform.up;
            var down = rotation * -transform.up;
            int castID = 0;

            CollectionUtil.GetPooled(out List<HitResult> results);

            if (CastsInDirection(CastDirection.Forward))
                ArrayCast(ref castID, results, center, forward, right, up, context);

            if (CastsInDirection(CastDirection.Back))
                ArrayCast(ref castID, results, center, back, left, up, context);

            if (CastsInDirection(CastDirection.Right))
                ArrayCast(ref castID, results, center, right, back, up, context);

            if (CastsInDirection(CastDirection.Left))
                ArrayCast(ref castID, results, center, left, forward, up, context);

            if (CastsInDirection(CastDirection.Up))
                ArrayCast(ref castID, results, center, up, right, back, context);

            if (CastsInDirection(CastDirection.Down))
                ArrayCast(ref castID, results, center, down, right, forward, context);

            var finalResult = new SweepResult(results);

            return finalResult;
        }

        private SweepResult OverlapSweep(in SweepContext context)
        {
            var center = transform.TransformPoint(positionOffset);

            CollectionUtil.GetPooled(out List<HitResult> results);

            int hits = Physics.OverlapSphereNonAlloc(
                center,
                ScaledRadius,
                colliderResults,
                context.CollisionMask,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < hits; i++)
            {
                var collider = colliderResults[i];
                var heading = collider.transform.position - center;
                var distance = heading.magnitude;
                var direction = heading / distance;
                var hit = new HitResult(
                    0,
                    collider,
                    collider.transform.position,
                    -direction,
                    distance
                );

                results.Add(hit);
            }

            return new(results);
        }

        private void ArrayCast(
            ref int id,
            List<HitResult> results,
            Vector3 center,
            Vector3 direction,
            Vector3 offsetDirectionX,
            Vector3 offsetDirectionY,
            in SweepContext context
        )
        {
            float radius = ScaledRadius;
            float offsetRadius = radius - EDGE_OFFSET;
            float diameter = 2.0f * radius;
            var startOffset = -radius * (offsetDirectionX + offsetDirectionY);

            for (int column = 0; column < gridRayCount; column++)
            {
                for (int row = 0; row < gridRayCount; row++)
                {
                    float tX = (float)row / (gridRayCount - 1);
                    float tY = (float)column / (gridRayCount - 1);

                    var offset =
                        startOffset
                        + diameter * ((tX * offsetDirectionX) + (tY * offsetDirectionY));
                    float distance = Mathf.Sqrt(radius * radius - offset.sqrMagnitude);
                    var origin = center + offset + (distance * -direction);
                    distance *= 2.0f;

                    int hits = Physics.RaycastNonAlloc(
                        origin,
                        direction,
                        rayResults,
                        distance,
                        context.CollisionMask,
                        QueryTriggerInteraction.Collide
                    );

                    if (hits == 0)
                        continue;

                    CollectionUtil.GetPooled(out List<int> sortedHits);

                    for (int i = 0; i < hits; i++)
                        sortedHits.Add(i);

                    sortedHits.Sort(
                        (h1, h2) => rayResults[h1].distance.CompareTo(rayResults[h2].distance)
                    );

                    foreach (var hitIndex in sortedHits)
                    {
                        var hit = rayResults[hitIndex];
                        results.Add(new(id, hit.collider, hit.point, hit.normal, hit.distance));
                    }

                    CollectionUtil.ReleasePooled(sortedHits);
                    id++;
                }
            }
        }

        private bool CastsInDirection(CastDirection direction)
        {
            return (castDirections & direction) != 0;
        }

        protected override void DrawGizmos()
        {
            var matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                transform.lossyScale
            );
            Gizmos.matrix *= Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(rotationOffset),
                Vector3.one
            );

            if (gizmoSettings.OutlineColor.HasValue)
            {
                Gizmos.color = gizmoSettings.OutlineColor.Value.With(a: enabled ? 1.0f : 0.15f);
                Gizmos.DrawWireSphere(positionOffset, radius);
            }

            if (!gizmoSettings.RayColor.HasValue)
                return;

            Gizmos.color = gizmoSettings.RayColor.Value.With(a: enabled ? 1.0f : 0.15f);

            if (detectionType == DetectionType.Center)
                DrawCenterCast();
            else if (detectionType == DetectionType.Grid)
            {
                if (CastsInDirection(CastDirection.Forward))
                    DrawArrayCast(Vector3.forward, Vector3.right, Vector3.up);

                if (CastsInDirection(CastDirection.Back))
                    DrawArrayCast(Vector3.back, Vector3.left, Vector3.up);

                if (CastsInDirection(CastDirection.Right))
                    DrawArrayCast(Vector3.right, Vector3.back, Vector3.up);

                if (CastsInDirection(CastDirection.Left))
                    DrawArrayCast(Vector3.left, Vector3.forward, Vector3.up);

                if (CastsInDirection(CastDirection.Up))
                    DrawArrayCast(Vector3.up, Vector3.right, Vector3.forward);

                if (CastsInDirection(CastDirection.Down))
                    DrawArrayCast(Vector3.down, Vector3.right, Vector3.back);
            }

            Gizmos.matrix = matrix;
        }

        private void DrawCenterCast()
        {
            var center = positionOffset;
            float xStep = 360.0f / centerRayCount;
            float yStep = 360.0f / centerRayCount;
            float xDegrees = isHalfSphere ? 180.0f : 0.0f;
            float yDegrees = 0.0f;

            while (xDegrees < 360.0f)
            {
                while (yDegrees < 360.0f)
                {
                    var rotation = Quaternion.Euler(new(xDegrees, yDegrees, 0.0f));
                    var direction = rotation * Vector3.forward;

                    Gizmos.DrawRay(center, radius * direction);

                    yDegrees += yStep;
                }

                yDegrees = 0.0f;
                xDegrees += xStep;
            }
        }

        private void DrawArrayCast(
            Vector3 direction,
            Vector3 offsetDirectionX,
            Vector3 offsetDirectionY
        )
        {
            var center = positionOffset;
            float radius = this.radius - EDGE_OFFSET;
            float diameter = 2.0f * radius;
            var startOffset = -radius * (offsetDirectionX + offsetDirectionY);

            for (int column = 0; column < gridRayCount; column++)
            {
                for (int row = 0; row < gridRayCount; row++)
                {
                    float tX = (float)row / (gridRayCount - 1);
                    float tY = (float)column / (gridRayCount - 1);

                    var offset =
                        startOffset
                        + diameter * ((tX * offsetDirectionX) + (tY * offsetDirectionY));
                    float distance = Mathf.Sqrt(this.radius * this.radius - offset.sqrMagnitude);

                    var origin = center + offset + (distance * -direction);
                    distance *= 2.0f;

                    Gizmos.DrawRay(origin, distance * direction);
                }
            }
        }
    }
}
