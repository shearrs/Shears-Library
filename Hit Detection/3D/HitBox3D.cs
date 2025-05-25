using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox3D : HitBody3D
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawRays = false;
        [SerializeField] private bool drawHits = false;
        [SerializeField] private bool drawHitAverage = false;
        [SerializeField] private bool drawActivity = false;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerFace = 8;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 orientation = Vector3.zero;
        [SerializeField] private Vector3 size = Vector3.one;

        private readonly List<RaycastHit> recentHits = new();
        private readonly RaycastHit[] results = new RaycastHit[10];

        private Vector3 Center => transform.position + center;
        private Quaternion Orientation => transform.localRotation * Quaternion.Euler(orientation);
        private Vector3 Size => Vector3.Scale(size, transform.lossyScale);

        protected override void Sweep()
        {
            if (drawActivity)
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
            newMatrix *= Matrix4x4.TRS(center, Quaternion.Euler(orientation), Vector3.one);

            Gizmos.matrix = newMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = originalMatrix;

            if (drawRays)
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

            if (drawActivity && activityDrawTick)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Center, Size);

                activityDrawTick = false;
            }

            if (drawHitAverage)
            {
                Vector3 averageHitPosition = Vector3.zero;

                foreach (var hit in recentHits)
                    averageHitPosition += hit.point;

                averageHitPosition /= recentHits.Count;

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(averageHitPosition, 0.25f);
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