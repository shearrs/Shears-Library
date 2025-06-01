using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public class SphereDetector : AreaDetector3D
    {
        [Header("Sphere Settings")]
        [field: SerializeField] public float Radius { get; set; } = 1;
        [field: SerializeField] public Vector3 Offset { get; set; }

        protected override int Sweep(Collider[] detections)
        {
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
            var radius = Radius * averageScale;

            int hits = Physics.OverlapSphereNonAlloc(transform.TransformPoint(Offset), radius, detections, DetectionMask, TriggerInteraction);

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            var averageScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
            var radius = Radius * averageScale;

            Gizmos.DrawWireSphere(transform.TransformPoint(Offset), radius);
        }
    }
}
