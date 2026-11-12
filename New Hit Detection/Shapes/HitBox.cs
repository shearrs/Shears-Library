using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox : HitShape
    {
#if UNITY_EDITOR
        [SerializeField, ShowIf(DRAW_GIZMOS_FLAG)]
        private GizmoSettings gizmoSettings = new();
#endif

        [Header("Transform")]
        [SerializeField]
        private Vector3 positionOffset;

        [SerializeField]
        private Vector3 rotationOffset;

        [SerializeField]
        private Vector3 size = Vector3.one;

        [Header("Casts")]
        [SerializeField, Range(1, 500), RuntimeReadOnly]
        private int maxHits = 10;

        [SerializeField, Range(2, 32), RuntimeReadOnly]
        private int raysPerRow = 4;

        [SerializeField]
        private CastDirection castDirections =
            CastDirection.Up
            | CastDirection.Down
            | CastDirection.Left
            | CastDirection.Right
            | CastDirection.Forward
            | CastDirection.Back;

        private RaycastHit[] rayResults = Array.Empty<RaycastHit>();

        private Vector3 ScaledSize
        {
            get
            {
                var size = this.size;
                size.Scale(transform.lossyScale);

                return size;
            }
        }

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

        private void Awake()
        {
            CollectionUtil.GetPooled(maxHits, out rayResults);
        }

        private void OnDestroy()
        {
            if (rayResults.Length > 0)
                CollectionUtil.ReleasePooled(rayResults);
        }

        public override SweepResult Sweep(in SweepContext context)
        {
            return ArrayCastDirections(context);
        }

        private SweepResult ArrayCastDirections(in SweepContext context)
        {
            var center = transform.TransformPoint(positionOffset);
            var rotation = Quaternion.Euler(rotationOffset);
            var forward = rotation * transform.forward;
            var back = rotation * -transform.forward;
            var right = rotation * transform.right;
            var left = rotation * -transform.right;
            var up = rotation * transform.up;
            var down = rotation * -transform.up;
            var size = ScaledSize;
            var halfSize = 0.5f * size;
            int castID = 0;

            CollectionUtil.GetPooled(out List<HitResult> results);

            if (CastsInDirection(CastDirection.Forward))
            {
                var backStart =
                    center + (halfSize.z * back) + (halfSize.y * up) + (halfSize.x * left);
                var backEnd = backStart + (size.y * down);

                ArrayCast(
                    ref castID,
                    results,
                    backStart,
                    backEnd,
                    right,
                    size.x,
                    forward,
                    size.z,
                    context
                );
            }

            if (CastsInDirection(CastDirection.Back))
            {
                var frontStart =
                    center + (halfSize.z * forward) + (halfSize.y * up) + (halfSize.x * left);
                var frontEnd = frontStart + (size.y * down);

                ArrayCast(
                    ref castID,
                    results,
                    frontStart,
                    frontEnd,
                    right,
                    size.x,
                    back,
                    size.z,
                    context
                );
            }

            if (CastsInDirection(CastDirection.Right))
            {
                var leftStart =
                    center + (halfSize.x * left) + (halfSize.y * up) + (halfSize.z * forward);
                var leftEnd = leftStart + (size.y * down);

                ArrayCast(
                    ref castID,
                    results,
                    leftStart,
                    leftEnd,
                    back,
                    size.z,
                    right,
                    size.x,
                    context
                );
            }

            if (CastsInDirection(CastDirection.Left))
            {
                var rightStart =
                    center + (halfSize.x * right) + (halfSize.y * up) + (halfSize.z * forward);
                var rightEnd = rightStart + (size.y * down);

                ArrayCast(
                    ref castID,
                    results,
                    rightStart,
                    rightEnd,
                    back,
                    size.z,
                    left,
                    size.x,
                    context
                );
            }

            if (CastsInDirection(CastDirection.Down))
            {
                var upStart =
                    center + (halfSize.y * up) + (halfSize.x * left) + (halfSize.z * forward);
                var upEnd = upStart + (size.z * back);

                ArrayCast(
                    ref castID,
                    results,
                    upStart,
                    upEnd,
                    right,
                    size.x,
                    down,
                    size.y,
                    context
                );
            }

            if (CastsInDirection(CastDirection.Up))
            {
                var downStart =
                    center + (halfSize.y * down) + (halfSize.x * left) + (halfSize.z * forward);
                var downEnd = downStart + (size.z * back);

                ArrayCast(
                    ref castID,
                    results,
                    downStart,
                    downEnd,
                    right,
                    size.x,
                    up,
                    size.y,
                    context
                );
            }

            var finalResult = new SweepResult(results);

            return finalResult;
        }

        private void ArrayCast(
            ref int id,
            List<HitResult> results,
            Vector3 start,
            Vector3 end,
            Vector3 columnOffsetDirection,
            float columnOffsetDistance,
            Vector3 direction,
            float distance,
            in SweepContext context
        )
        {
            CollectionUtil.GetPooled(out List<int> sortedHits);

            for (int column = 0; column < raysPerRow; column++)
            {
                for (int row = 0; row < raysPerRow; row++)
                {
                    float tY = (float)row / (raysPerRow - 1);
                    float tX = (float)column / (raysPerRow - 1);

                    var origin =
                        Vector3.Lerp(start, end, tY)
                        + (columnOffsetDistance * tX * columnOffsetDirection);

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

                    sortedHits.Clear();

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

                    id++;
                }
            }

            CollectionUtil.ReleasePooled(sortedHits);
        }

        private bool CastsInDirection(CastDirection direction)
        {
            return (castDirections & direction) != 0;
        }

        protected override void DrawGizmos()
        {
#if UNITY_EDITOR
            var matrix = Gizmos.matrix;
            var newMatrix = Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                transform.lossyScale
            );
            newMatrix *= Matrix4x4.TRS(
                positionOffset,
                Quaternion.Euler(rotationOffset),
                Vector3.one
            );
            Gizmos.matrix = newMatrix;

            if (gizmoSettings.OutlineColor.HasValue)
            {
                var outlineColor = gizmoSettings.OutlineColor.Value.With(a: enabled ? 1.0f : 0.15f);
                Gizmos.color = outlineColor;
                Gizmos.DrawWireCube(Vector3.zero, size);
            }

            if (gizmoSettings.RayColor.HasValue)
            {
                var center = Vector3.zero;
                var halfSize = 0.5f * size;
                var forward = Vector3.forward;
                var back = Vector3.back;
                var left = Vector3.left;
                var right = Vector3.right;
                var up = Vector3.up;
                var down = Vector3.down;

                var backStart =
                    center + (halfSize.z * back) + (halfSize.y * up) + (halfSize.x * left);
                var backEnd = backStart + (size.y * down);

                var frontStart =
                    center + (halfSize.z * forward) + (halfSize.y * up) + (halfSize.x * left);
                var frontEnd = frontStart + (size.y * down);

                var leftStart =
                    center + (halfSize.x * left) + (halfSize.y * up) + (halfSize.z * forward);
                var leftEnd = leftStart + (size.y * down);

                var rightStart =
                    center + (halfSize.x * right) + (halfSize.y * up) + (halfSize.z * forward);
                var rightEnd = rightStart + (size.y * down);

                var upStart =
                    center + (halfSize.y * up) + (halfSize.x * left) + (halfSize.z * forward);
                var upEnd = upStart + (size.z * back);

                var downStart =
                    center + (halfSize.y * down) + (halfSize.x * left) + (halfSize.z * forward);
                var downEnd = downStart + (size.z * back);

                if (CastsInDirection(CastDirection.Forward))
                    DrawArrayCast(backStart, backEnd, right, size.x, forward, size.z);

                if (CastsInDirection(CastDirection.Back))
                    DrawArrayCast(frontStart, frontEnd, right, size.x, back, size.z);

                if (CastsInDirection(CastDirection.Right))
                    DrawArrayCast(leftStart, leftEnd, back, size.z, right, size.x);

                if (CastsInDirection(CastDirection.Left))
                    DrawArrayCast(rightStart, rightEnd, back, size.z, left, size.x);

                if (CastsInDirection(CastDirection.Down))
                    DrawArrayCast(upStart, upEnd, right, size.x, down, size.y);

                if (CastsInDirection(CastDirection.Up))
                    DrawArrayCast(downStart, downEnd, right, size.x, up, size.y);
            }

            Gizmos.matrix = matrix;
#endif
        }

        private void DrawArrayCast(
            Vector3 start,
            Vector3 end,
            Vector3 columnOffsetDirection,
            float columnOffsetDistance,
            Vector3 direction,
            float distance
        )
        {
            var rayColor = gizmoSettings.RayColor.Value.With(a: enabled ? 1.0f : 0.15f);
            var arrowColor = Color.magenta.With(a: enabled ? 1.0f : 0.15f);

            Gizmos.color = rayColor;

            for (int column = 0; column < raysPerRow; column++)
            {
                for (int row = 0; row < raysPerRow; row++)
                {
                    float tX = (float)column / (raysPerRow - 1);
                    float tY = (float)row / (raysPerRow - 1);

                    var origin =
                        Vector3.Lerp(start, end, tY)
                        + (columnOffsetDistance * tX * columnOffsetDirection);
                    var crossAgainst =
                        (direction == Vector3.up || direction == Vector3.down)
                            ? Vector3.forward
                            : Vector3.up;

                    GizmosUtil.DrawArrow(
                        origin,
                        distance * direction,
                        Vector3.Cross(direction, crossAgainst),
                        0.075f,
                        0.075f,
                        arrowColor
                    );
                }
            }
        }
    }
}
