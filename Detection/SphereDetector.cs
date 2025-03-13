using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public class SphereDetector : AreaDetector
    {
        [Header("Sphere Settings")]
        [field: SerializeField] public float Radius { get; set; } = 1;
        [field: SerializeField] public Vector3 LocalOffset { get; set; }

        protected override int Sweep(Collider[] detections)
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.TransformPoint(LocalOffset), Radius, detections, DetectionMask, TriggerInteraction);

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(LocalOffset), Radius);
        }
    }
}
