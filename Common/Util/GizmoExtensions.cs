using UnityEngine;

namespace Shears
{
    public static class GizmoExtensions
    {
        public static void DrawWireCapsule(Vector3 position, float radius, float height)
        {
            Vector3 p1 = position;
            Vector3 p2 = position + (height * Vector3.up);

            Vector3 rightOffset = Vector3.right * radius;
            Vector3 forwardOffset = Vector3.forward * radius;

            DrawWireCapsule(p1, p2, radius, rightOffset, forwardOffset);
        }

        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
            Vector3 up = (p2 - p1);
            Vector3 right = Vector3.zero;
            Vector3 forward = Vector3.zero;
            Vector3.OrthoNormalize(ref up, ref right, ref forward);

            Vector3 rightOffset = right * radius;
            Vector3 forwardOffset = forward * radius;

            DrawWireCapsule(p1, p2, radius, rightOffset, forwardOffset);
        }

        private static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius, Vector3 rightOffset, Vector3 forwardOffset)
        {
            Gizmos.DrawWireSphere(p1, radius);
            Gizmos.DrawWireSphere(p2, radius);

            Gizmos.DrawLine(p1 + rightOffset, p2 + rightOffset);
            Gizmos.DrawLine(p1 - rightOffset, p2 - rightOffset);
            Gizmos.DrawLine(p1 + forwardOffset, p2 + forwardOffset);
            Gizmos.DrawLine(p1 - forwardOffset, p2 - forwardOffset);
        }
    }
}
