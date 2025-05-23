using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitResult2D : IHitResult
    {
        private readonly Vector2 centroid;
        private readonly Vector2 point;
        private readonly Vector2 normal;
        private readonly float distance;
        private readonly Transform transform;
        private readonly Collider2D collider;

        readonly Vector3 IHitResult.Point => point;
        readonly Vector3 IHitResult.Normal => normal;
        public readonly Vector2 Centroid => centroid;
        public readonly float Distance => distance;
        public readonly Transform Transform => transform;
        public readonly Collider2D Collider => collider;

        public HitResult2D(RaycastHit2D hit)
        {
            centroid = hit.centroid;
            point = hit.point;
            normal = hit.normal;
            distance = hit.distance;
            transform = hit.collider.transform;
            collider = hit.collider;
        }
    }
}
