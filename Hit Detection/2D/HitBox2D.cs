using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox2D : HitBody2D
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawRays = false;
        [SerializeField] private bool drawHits = false;
        [SerializeField] private bool drawHitAverage = false;
        [SerializeField] private bool drawActivity = false;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerSide = 8;
        [SerializeField] private Vector2 center;
        [SerializeField] private float angle = 0f;
        [SerializeField] private Vector2 size = Vector2.one;

        private readonly List<RaycastHit2D> recentHits = new();
        private readonly RaycastHit2D[] results = new RaycastHit2D[50];

        private Vector2 Center => transform.TransformPoint(center);
        private Quaternion Orientation => transform.localRotation * Quaternion.Euler(new(0, 0, angle));
        private Vector2 Size => size * transform.lossyScale;

        protected override void Sweep()
        {
            if (drawActivity)
                activityDrawTick = true;

            recentHits.Clear();

            Vector2 halfSize = Size * 0.5f;
            Vector2 left = Orientation * Vector2.left;
            Vector2 right = Orientation * Vector2.right;
            Vector2 up = Orientation * Vector2.up;
            Vector2 down = Orientation * Vector2.down;

            Vector2 leftStart = Center + (left * halfSize.x) + (up * halfSize.y);
            Vector2 leftEnd = leftStart + (down * Size.y);

            Vector2 rightStart = Center + (right * halfSize.x) + (up * halfSize.y);
            Vector2 rightEnd = rightStart + (down * Size.y);

            Vector2 bottomStart = Center + (left * halfSize.x) + (down * halfSize.y);
            Vector2 bottomEnd = bottomStart + (right * Size.x);

            Vector2 topStart = Center + (left * halfSize.x) + (up * halfSize.y);
            Vector2 topEnd = topStart + (right * Size.x);

            ArrayCast(leftStart, leftEnd, right, Size.x);
            ArrayCast(rightStart, rightEnd, left, Size.x);
            ArrayCast(bottomStart, bottomEnd, up, Size.y);
            ArrayCast(topStart, topEnd, down, Size.y);
        }

        private void ArrayCast(Vector2 start, Vector2 end, Vector2 direction, float distance)
        {
            var filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = true,
                layerMask = collisionMask,
            };

            for (int i = 0; i < raysPerSide; i++)
            {
                float t = (float)i / (raysPerSide - 1);
                Vector2 origin = Vector2.Lerp(start, end, t);

                int hits = Physics2D.Raycast(origin, direction, filter, results, distance);

                if (hits > 0)
                    AddValidHits(hits);
            }
        }

        private void AddValidHits(int hits)
        {
            for (int i = 0; i < hits; i++)
            {
                RaycastHit2D result = results[i];

                if (result.point == Vector2.zero || result.collider == null)
                    continue;

                recentHits.Add(result);

                if (finalHits.TryGetValue(result.collider, out var oldHit))
                {
                    if (oldHit.distance < result.distance)
                        finalHits[result.collider] = result;
                }
                else
                    finalHits.Add(result.collider, result);

                if (drawHits)
                    Debug.DrawLine(Center, result.point, Color.red);
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled || !drawGizmos)
                return;

            var originalMatrix = Gizmos.matrix;
            var newMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            newMatrix *= Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);

            Gizmos.matrix = newMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = originalMatrix;

            if (drawRays)
            {
                Vector2 halfSize = Size * 0.5f;
                Vector2 left = Orientation * Vector2.left;
                Vector2 right = Orientation * Vector2.right;
                Vector2 up = Orientation * Vector2.up;
                Vector2 down = Orientation * Vector2.down;

                Vector2 leftStart = Center + (left * halfSize.x) + (up * halfSize.y);
                Vector2 leftEnd = leftStart + (down * Size.y);

                Vector2 rightStart = Center + (right * halfSize.x) + (up * halfSize.y);
                Vector2 rightEnd = rightStart + (down * Size.y);

                Vector2 bottomStart = Center + (left * halfSize.x) + (down * halfSize.y);
                Vector2 bottomEnd = bottomStart + (right * Size.x);

                Vector2 topStart = Center + (left * halfSize.x) + (up * halfSize.y);
                Vector2 topEnd = topStart + (right * Size.x);

                DrawArrayCast(leftStart, leftEnd, right, Size.x);
                DrawArrayCast(rightStart, rightEnd, left, Size.x);
                DrawArrayCast(bottomStart, bottomEnd, up, Size.y);
                DrawArrayCast(topStart, topEnd, down, Size.y);
            }

            if (drawActivity && activityDrawTick)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Center, Size);

                activityDrawTick = false;
            }

            if (drawHitAverage)
            {
                Vector2 averageHitPosition = Vector2.zero;

                foreach (var hit in recentHits)
                    averageHitPosition += hit.point;

                averageHitPosition /= recentHits.Count;

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(averageHitPosition, 0.25f);
            }
        }

        private void DrawArrayCast(Vector2 start, Vector2 end, Vector2 direction, float distance)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < raysPerSide; i++)
            {
                float t = (float)i / (raysPerSide - 1);
                Vector2 origin = Vector2.Lerp(start, end, t);

                Gizmos.DrawRay(origin, direction * distance);
            }
        }
    }
}
