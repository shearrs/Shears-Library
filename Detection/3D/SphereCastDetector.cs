using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public class SphereCastDetector : AreaDetector3D
    {
        [field: SerializeField] public float Radius { get; set; } = 1;
        [field: SerializeField] public Vector3 Origin { get; set; } = Vector3.zero;
        [field: SerializeField] public Vector3 Direction { get; set; } = Vector3.forward;
        [field: SerializeField] public float Distance { get; set; } = 3f;

        private readonly RaycastHit[] raycastHits = new RaycastHit[10];

        public IReadOnlyList<RaycastHit> RaycastHits => raycastHits;

        protected override int Sweep(Collider[] detections)
        {
            Vector3 origin = transform.TransformPoint(Origin);
            Vector3 direction = transform.TransformDirection(Direction);

            int hits = Physics.SphereCastNonAlloc(origin, Radius, direction, raycastHits, Distance, DetectionMask, TriggerInteraction);

            for (int i = 0; i < hits; i++)
            {
                detections[i] = raycastHits[i].collider;
            }

            return hits;
        }

        protected override void DrawWireGizmos()
        {
            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Vector3 p1 = Origin;
            Vector3 p2 = Origin + (Distance * Direction);

            GizmoExtensions.DrawWireCapsule(p1, p2, Radius);
            
            Gizmos.matrix = matrix;
        }
    }
}
