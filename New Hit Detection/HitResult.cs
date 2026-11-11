using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitResult
    {
        public int ResultID { get; }
        public Collider Collider { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public float Distance { get; }

        public HitResult(
            int resultID,
            Collider collider,
            Vector3 hitPoint,
            Vector3 hitNormal,
            float distance
        )
        {
            ResultID = resultID;
            Collider = collider;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Distance = distance;
        }
    }
}
