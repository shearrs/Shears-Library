using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HitSphere : HitBody3D
    {
        [Header("Gizmo Settings")]
        [SerializeField] private bool drawGizmos = true;

        [Header("Collision Settings")]
        [SerializeField, Range(0, 100)] private int maxHits = 10;
        [SerializeField, Min(0.1f)] private float radius = 0.5f;
        [SerializeField] private Vector3 center;

        private readonly Dictionary<Collider, List<RaycastHit>> recentHits = new();
        private Collider[] results;
        private Vector3 debugPoint;
        private Vector3 debugOrigin;
        private bool debugHitThisFrame;

        private Vector3 Center => transform.TransformPoint(center);
        private float Radius => radius * (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3;

        protected override void Awake()
        {
            base.Awake();

            results = new Collider[maxHits];
        }

        public Vector3 GetAverageHitNormal(Collider collider)
        {
            if (!recentHits.ContainsKey(collider) || recentHits[collider].Count == 0)
                return Vector3.zero;

            Vector3 averageHitPosition = Vector3.zero;

            foreach (var hit in recentHits[collider])
                averageHitPosition += hit.point;

            averageHitPosition /= recentHits[collider].Count;

            return averageHitPosition;
        }

        protected override void Sweep()
        {
            int hits = Physics.OverlapSphereNonAlloc(Center, Radius, results, collisionMask, QueryTriggerInteraction.Collide);

            AddValidHits(hits);
        }

        private void AddValidHits(int hits)
        {
            debugHitThisFrame = false;

            for (int i = 0; i < hits; i++)
            {
                Collider result = results[i];
                
                if (ignoreList.Contains(result) || finalHits.ContainsKey(result))
                    continue;

                Vector3 closestPoint = result.ClosestPoint(Center);

                Vector3 frontOrigin = Vector3.forward * radius;
                Vector3 sideOrigin = Vector3.right * radius;
                Vector3 topOrigin = Vector3.up * radius;

                RaycastHit hit = CastFromOrigin(center - frontOrigin, closestPoint);
                if (hit.transform == null) hit = CastFromOrigin(center + frontOrigin, closestPoint);
                if (hit.transform == null) hit = CastFromOrigin(center + sideOrigin, closestPoint);
                if (hit.transform == null) hit = CastFromOrigin(center - sideOrigin, closestPoint);
                if (hit.transform == null) hit = CastFromOrigin(center + topOrigin, closestPoint);
                if (hit.transform == null) hit = CastFromOrigin(center - topOrigin, closestPoint);

                if (hit.transform != null && !finalHits.ContainsKey(hit.collider))
                {
                    debugHitThisFrame = true;
                    finalHits.Add(hit.collider, hit);
                }
            }
        }

        private RaycastHit CastFromOrigin(Vector3 origin, Vector3 closestPoint)
        {
            origin = transform.TransformPoint(origin);
            Vector3 heading = closestPoint - origin;
            Vector3 direction = heading.normalized;
            float distance = heading.magnitude + 0.015f;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, collisionMask, QueryTriggerInteraction.Collide);
            RaycastHit hit = default;

            for (int i = 0; i < hits.Length; i++)
            {
                if (ignoreList.Contains(hits[i].collider))
                    continue;

                if (hits[i].transform != null)
                {
                    debugPoint = closestPoint;
                    debugOrigin = origin;

                    hit = hits[i];
                    break;
                }
            }

            return hit;
        }

        private void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled || !drawGizmos)
                return;

            Matrix4x4 matrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, Radius);

            Vector3 frontOrigin = Vector3.forward * Radius;
            Vector3 sideOrigin = Vector3.right * Radius;
            Vector3 topOrigin = Vector3.up * Radius;

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(center + frontOrigin, 0.015f);
            Gizmos.DrawSphere(center - frontOrigin, 0.015f);
            Gizmos.DrawSphere(center + sideOrigin, 0.015f);
            Gizmos.DrawSphere(center - sideOrigin, 0.015f);
            Gizmos.DrawSphere(center + topOrigin, 0.015f);
            Gizmos.DrawSphere(center - topOrigin, 0.015f);

            Gizmos.matrix = matrix;

            if (debugHitThisFrame && debugOrigin != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(debugOrigin, debugPoint);
            }
        }
    }
}