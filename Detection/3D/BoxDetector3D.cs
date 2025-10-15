using UnityEngine;

namespace Shears.Detection
{
    public class BoxDetector3D : AreaDetector3D
    {
        [Header("Box Settings")]
        [SerializeField] private Vector3 halfExtents = 0.5f * Vector3.one;
        [SerializeField] private Vector3 offset;

        public Vector3 HalfExtents { get => halfExtents; set => halfExtents = value; }
        public Vector3 Offset { get => offset; set => offset = value; } 

        protected override int Sweep(Collider[] detections)
        {
            Vector3 scaledSize = halfExtents.MultiplyComponents(transform.lossyScale);

            int hits = Physics.OverlapBoxNonAlloc(transform.TransformPoint(Offset), scaledSize, detections, transform.rotation, DetectionMask, TriggerInteraction);

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            var matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Gizmos.DrawWireCube(offset, 2.0f * halfExtents);

            Gizmos.matrix = matrix;
        }
    }
}
