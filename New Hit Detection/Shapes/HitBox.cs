using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox : HitShape
    {
        [SerializeField, ShowIf(DRAW_GIZMOS_FLAG)]
        private Color gizmoOutlineColor = Color.red;

        [SerializeField, ShowIf(DRAW_GIZMOS_FLAG)]
        private Color rayColor = Color.yellow;

        [Header("Transform")]
        [SerializeField]
        private Vector3 size = Vector3.one;

        [SerializeField]
        private Vector3 positionOffset;

        [SerializeField]
        private Vector3 rotationOffset;

        [Header("Casts")]
        [SerializeField, Range(1, 500), RuntimeReadOnly]
        private int maxHits = 10;

        [SerializeField, Range(4, 128), RuntimeReadOnly]
        private int raysPerFace = 16;

        [SerializeField]
        private CastDirection castDirections =
            CastDirection.Up
            | CastDirection.Down
            | CastDirection.Left
            | CastDirection.Right
            | CastDirection.Forward
            | CastDirection.Back;

        private RaycastHit[] rayResults = Array.Empty<RaycastHit>();

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
            var halfSize = 0.5f * size;
            int castID = 0;

            CollectionUtil.GetPooled(out List<HitResult> results);

            if ((castDirections & CastDirection.Forward) != 0)
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

            if ((castDirections & CastDirection.Back) != 0)
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

            if ((castDirections & CastDirection.Right) != 0)
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

            if ((castDirections & CastDirection.Left) != 0)
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

            if ((castDirections & CastDirection.Down) != 0)
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

            if ((castDirections & CastDirection.Up) != 0)
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
            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tY = (float)row / (raysPerFace - 1);
                    float tX = (float)column / (raysPerFace - 1);

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

                    id++;

                    foreach (var hit in rayResults)
                        results.Add(new(id, hit.collider, hit.point, hit.normal, hit.distance));
                }
            }
        }

        protected override void DrawGizmos()
        {
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

            Gizmos.color = gizmoOutlineColor;
            Gizmos.DrawWireCube(Vector3.zero, size);

            var center = transform.TransformPoint(positionOffset);
            var rotation = Quaternion.Euler(rotationOffset);
            var forward = rotation * transform.forward;
            var back = rotation * -transform.forward;
            var right = rotation * transform.right;
            var left = rotation * -transform.right;
            var up = rotation * transform.up;
            var down = rotation * -transform.up;
            var halfSize = 0.5f * size;
            var forwardStart =
                center + (halfSize.z * forward) + (halfSize.x * left) + (halfSize.y * up);
            var forwardEnd = forwardStart + (size.y * down);

            for (int column = 0; column < raysPerFace; column++)
            {
                for (int row = 0; row < raysPerFace; row++)
                {
                    float tX = (float)column / (raysPerFace - 1);
                    float tY = (float)row / (raysPerFace - 1);

                    var origin = Vector3.Lerp(forwardStart, forwardEnd, tY) + (size.x * tX * right);
                    var crossAgainst =
                        (back == Vector3.up || back == Vector3.down) ? Vector3.forward : Vector3.up;

                    GizmosUtil.DrawArrow(
                        origin,
                        back * size.z,
                        Vector3.Cross(back, crossAgainst),
                        0.075f,
                        0.075f,
                        Color.magenta
                    );
                }
            }

            Gizmos.matrix = matrix;
        }
    }
}
