using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitBox3D : HitBody3D
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawActivity = false;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField, Range(2, 32)] private int raysPerFace = 8;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 orientation = Vector3.zero;
        [SerializeField] private Vector3 halfExtents = new(0.5f, 0.5f, 0.5f);

        private readonly List<RaycastHit> recentHits = new();
        private readonly RaycastHit[] results = new RaycastHit[10];

        private Vector3 Center => transform.position + center;
        private Quaternion Orientation => transform.localRotation * Quaternion.Euler(orientation);
        private Vector3 HalfExtents => Vector3.Scale(halfExtents, transform.lossyScale);

        protected override void Sweep()
        {
            if (drawActivity)
                activityDrawTick = true;

            recentHits.Clear();

            Vector3 extents = HalfExtents * 2;

            Vector3 backStart = Center + (Vector3.back * HalfExtents.z) + (Vector3.up * HalfExtents.y);
            Vector3 backEnd = backStart + (Vector3.down * extents.y);

            Vector3 frontStart = Center + (Vector3.forward * HalfExtents.z) + (Vector3.up * HalfExtents.y);
            Vector3 frontEnd = frontStart + (Vector3.down * extents.y);
        }

        private void ArrayCast(Vector3 start, Vector3 end, Vector3 direction, float distance)
        {

        }

        private void AddValidHits(int hits)
        {
            for (int i = 0; i < hits; i++)
            {
                RaycastHit result = results[i];

                if (result.point != Vector3.zero && result.collider != null && !finalHits.ContainsKey(result.collider))
                    finalHits.Add(result.collider, result);
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled || !drawGizmos)
                return;
        }
    }
}