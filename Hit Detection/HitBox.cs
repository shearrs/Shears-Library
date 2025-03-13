using UnityEngine;

namespace SH.Combat.HitDetection
{
    public class HitBox : HitBody
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawSkin = false;
        [SerializeField] private bool drawDirection = true;
        [SerializeField] private bool drawActivity = false;
        private bool activityDrawTick = false;

        [Header("Collision Settings")]
        [SerializeField] private Vector3 center;
        [SerializeField] private Quaternion orientation = Quaternion.identity;
        [SerializeField] private Vector3 halfExtents = new(0.5f, 0.5f, 0.5f);
        [SerializeField, Range(0.02f, 0.15f)] private float skinWidth = 0.025f;

        private readonly RaycastHit[] results = new RaycastHit[10];

        #region Shortcut Properties
        // hitbox
        private Vector3 HalfExtents => Vector3.Scale(halfExtents, transform.lossyScale);
        private Quaternion Orientation => transform.localRotation * orientation;
        #endregion

        protected override void Sweep()
        {
            if (drawActivity)
                activityDrawTick = true;

            Vector3 halfExtentsScaled = HalfExtents;
            float halfSkinWidth = skinWidth * 0.5f;

            Vector3 skinOffsets = new(halfExtents.x - halfSkinWidth, halfExtents.y - halfSkinWidth, halfExtents.z - halfSkinWidth);

            Vector3 forwardSkinOffset = Vector3.forward * skinOffsets.z;
            Vector3 forwardSkinHalfExtents = new(halfExtentsScaled.x, halfExtentsScaled.y, halfSkinWidth);
            Vector3 forwardDirection = Orientation * Vector3.forward;
            float forwardDistance = (halfExtentsScaled.z * 2) - halfSkinWidth;

            Vector3 sideSkinOffset = Vector3.right * skinOffsets.x;
            Vector3 sideSkinHalfExtents = new(halfSkinWidth, halfExtentsScaled.y, halfExtentsScaled.z);
            Vector3 sideDirection = Orientation * Vector3.right;
            float sideDistance = (halfExtentsScaled.x * 2) - halfSkinWidth;

            Vector3 topSkinOffset = Vector3.up * skinOffsets.y;
            Vector3 topSkinHalfExtents = new(halfExtentsScaled.x, halfSkinWidth, halfExtentsScaled.z);
            Vector3 topDirection = Orientation * Vector3.up;
            float topDistance = (halfExtentsScaled.y * 2) - halfSkinWidth;

            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, Orientation, transform.lossyScale);

            Debug.DrawRay(matrix.MultiplyPoint(center - forwardSkinOffset), transform.rotation * Orientation * Vector3.forward * forwardDistance, Color.red);

            BoxCastHits(-forwardSkinOffset, forwardSkinHalfExtents, forwardDirection, forwardDistance);
            BoxCastHits(forwardSkinOffset, forwardSkinHalfExtents, -forwardDirection, forwardDistance);
            BoxCastHits(-sideSkinOffset, sideSkinHalfExtents, sideDirection, sideDistance);
            BoxCastHits(sideSkinOffset, sideSkinHalfExtents, -sideDirection, sideDistance);
            BoxCastHits(-topSkinOffset, topSkinHalfExtents, topDirection, topDistance);
            BoxCastHits(topSkinOffset, topSkinHalfExtents, -topDirection, topDistance);
        }

        private void BoxCastHits(Vector3 offset, Vector3 halfExtents, Vector3 direction, float distance)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, Orientation, transform.lossyScale);
            int hits = Physics.BoxCastNonAlloc(matrix.MultiplyPoint(center + offset), halfExtents, direction, results, Orientation, distance, collisionMask, QueryTriggerInteraction.Collide);

            AddValidHits(hits);
        }

        private void AddValidHits(int hits)
        {
            for (int i = 0; i < hits; i++)
            {
                RaycastHit result = results[i];

                if (result.point != Vector3.zero && !finalHits.Contains(result))
                    finalHits.Add(results[i]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!isActiveAndEnabled || !drawGizmos)
                return;

            float halfSkinWidth = skinWidth * 0.5f;

            Vector3 skinOffsets = new(halfExtents.x - halfSkinWidth, halfExtents.y - halfSkinWidth, halfExtents.z - halfSkinWidth);

            Vector3 extents = halfExtents * 2;
            Vector3 forwardSkinOffset = Vector3.forward * skinOffsets.z;
            Vector3 forwardSkinSize = new(extents.x, extents.y, skinWidth);
            Vector3 sideSkinOffset = Vector3.right * skinOffsets.x;
            Vector3 sideSkinSize = new(skinWidth, extents.y, extents.z);
            Vector3 topSkinOffset = Vector3.up * skinOffsets.y;
            Vector3 topSkinSize = new(extents.x, skinWidth, extents.z);

            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Orientation, transform.lossyScale);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, halfExtents * 2);

            if (drawSkin)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(center + forwardSkinOffset, forwardSkinSize);
                Gizmos.DrawWireCube(center - forwardSkinOffset, forwardSkinSize);
                Gizmos.DrawWireCube(center + sideSkinOffset, sideSkinSize);
                Gizmos.DrawWireCube(center - sideSkinOffset, sideSkinSize);
                Gizmos.DrawWireCube(center + topSkinOffset, topSkinSize);
                Gizmos.DrawWireCube(center - topSkinOffset, topSkinSize);
            }

            if (drawDirection)
            {
                Vector3 arrowHead1 = forwardSkinOffset + (Vector3.up * 0.1f) + (Vector3.back * 0.1f);
                Vector3 arrowHead2 = new(arrowHead1.x, -arrowHead1.y, arrowHead1.z);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(center - forwardSkinOffset, center + forwardSkinOffset);
                Gizmos.DrawLine(center + forwardSkinOffset, center + arrowHead1);
                Gizmos.DrawLine(center + forwardSkinOffset, center + arrowHead2);
            }

            if (drawActivity && activityDrawTick)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(center, halfExtents * 2);

                activityDrawTick = false;
            }

            Gizmos.matrix = originalMatrix;
        }
    }
}