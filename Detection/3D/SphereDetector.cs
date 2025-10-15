using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public class SphereDetector : AreaDetector3D
    {
        [Header("Sphere Settings")]
        [SerializeField] private float radius = 1.0f;
        [SerializeField] private Vector3 offset = Vector3.zero;

        public float Radius { get => radius; set => radius = value; }
        public Vector3 Offset { get => offset; set => offset = value; }

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
