using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitResult3D
    {
        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly float distance;
        public readonly Transform transform;
        public readonly Collider collider;

        public HitResult3D(
            Vector3 point,
            Vector3 normal,
            float distance,
            Transform transform,
            Collider collider
        )
        {
            this.point = point;
            this.normal = normal;
            this.distance = distance;
            this.transform = transform;
            this.collider = collider;
        }

        public HitResult3D(RaycastHit hit)
        {
            point = hit.point;
            normal = hit.normal;
            distance = hit.distance;
            transform = hit.collider.transform;
            collider = hit.collider;
        }
    }
}
