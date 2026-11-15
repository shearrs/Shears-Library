using UnityEngine;

namespace Shears.Controllers
{
    [DefaultExecutionOrder(-1000)]
    public class ShearsController : ShearsBehaviour
    {
        private const int SOLVE_STEPS = 8;
        private const float GROUND_BUFFER = 0.01f;

        [Header("Collision")]
        [SerializeField]
        private CapsuleCollider collider;

        [SerializeField]
        private LayerMask layerMask = 1;

        [SerializeField]
        private LayerMask groundLayerMask = 1;

        [SerializeField, Range(0.001f, 1.0f)]
        private float skinWidth = 0.1f;

        [SerializeField, Range(0.0f, 90.0f)]
        private float maxSlopeAngle = 45.0f;

        [SerializeField, Min(0)]
        private float maxSnapDistance = 0.5f;

        [Header("Flags")]
        [SerializeField, ReadOnly]
        private bool isGrounded;

        [SerializeField, ReadOnly]
        private Vector3 velocity;

        private readonly RaycastHit[] raycastHits = new RaycastHit[10];
        private readonly RaycastHit[] groundedHits = new RaycastHit[10];
        private readonly Collider[] overlaps = new Collider[2];

        public bool IsGrounded => isGrounded;
        public Vector3 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        private void Update()
        {
            UpdateIsGrounded();
        }

        public void Move(Vector3 movement)
        {
            Velocity = movement;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            ExitCollision();
            var finalMovement = CapsuleCast(Time.deltaTime * Velocity, transform.position);

            if (
                IsGrounded
                && finalMovement.y <= 0
                && TryGetGroundSnap(transform.position + finalMovement, out var snap)
            )
                finalMovement += snap;

            transform.position += finalMovement;
            Velocity = finalMovement / Time.deltaTime;

            UpdateIsGrounded();

            if (IsGrounded)
                Velocity = velocity.With(y: 0.0f);
        }

        public void ExitCollision()
        {
            float halfHeight = (0.5f * collider.height) - skinWidth;
            float radius = collider.radius - skinWidth;
            float radiusOffset = Mathf.Min(radius, halfHeight);
            var point1 =
                transform.position
                + collider.center.With(y: collider.center.y - (halfHeight - radiusOffset));
            var point2 =
                transform.position
                + collider.center.With(y: collider.center.y + (halfHeight - radiusOffset));

            int hits = Physics.OverlapCapsuleNonAlloc(
                point1,
                point2,
                radius,
                overlaps,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == 0)
                return;

            Collider overlap = null;

            for (int i = 0; i < hits; i++)
            {
                if (overlaps[i] != collider)
                {
                    overlap = overlaps[i];
                    break;
                }
            }

            if (overlap == null)
                return;

            if (
                Physics.ComputePenetration(
                    collider,
                    collider.transform.position,
                    collider.transform.rotation,
                    overlap,
                    overlap.transform.position,
                    overlap.transform.rotation,
                    out var direction,
                    out var distance
                )
            )
                transform.position += Mathf.Max(0, distance + 0.01f - skinWidth) * direction;
        }

        private Vector3 CapsuleCast(
            Vector3 movement,
            Vector3 origin,
            Vector3 previousNormal = default,
            int step = 0
        )
        {
            float halfHeight = (0.5f * collider.height) - skinWidth;
            float radius = collider.radius - skinWidth;
            float radiusOffset = Mathf.Min(radius, halfHeight);
            var point1 = collider.center.With(y: collider.center.y - (halfHeight - radiusOffset));
            var point2 = collider.center.With(y: collider.center.y + (halfHeight - radiusOffset));

            if (step == 0 && movement.y < 0 && IsGrounded)
                movement.y = 0;

            float distance = movement.magnitude;

            if (distance == 0.0f)
                return Vector3.zero;

            var direction = movement / distance;

            point1 = origin + point1;
            point2 = origin + point2;

            int hits = Physics.CapsuleCastNonAlloc(
                point1,
                point2,
                radius,
                direction,
                raycastHits,
                distance + skinWidth,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == 0)
                return movement;

            float minDistance = float.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < hits; i++)
            {
                if (raycastHits[i].collider == collider)
                    continue;

                float hitDistance = raycastHits[i].distance;

                if (hitDistance < minDistance)
                {
                    minDistance = hitDistance;
                    minIndex = i;
                }
            }

            if (minIndex == -1)
                return movement;

            minDistance = Mathf.Max(0, minDistance - skinWidth);
            var normal = raycastHits[minIndex].normal;
            var blockedMovement = minDistance * direction;

            if (step == SOLVE_STEPS)
                return blockedMovement;

            var remainingMovement = movement - blockedMovement;
            Vector3 projection;

            float angle = Vector3.Angle(normal, Vector3.up);

            if (
                step > 0
                && previousNormal != Vector3.zero
                && Vector3.Dot(normal, previousNormal) < 0.99f
            )
            {
                var creaseDirection = Vector3.Cross(previousNormal, normal).normalized;
                projection = Vector3.Project(remainingMovement, creaseDirection);
            }
            else if (angle > 0.0f && angle <= maxSlopeAngle)
            {
                var right = Vector3.Cross(remainingMovement, Vector3.up);
                var slopeDirection = Vector3.Cross(normal, right).normalized;
                projection = slopeDirection * remainingMovement.magnitude;
            }
            else
                projection = Vector3.ProjectOnPlane(remainingMovement, normal);

            if (projection.sqrMagnitude == 0.0f)
                return blockedMovement;
            else
                return blockedMovement
                    + CapsuleCast(projection, origin + blockedMovement, normal, step + 1);
        }

        private void UpdateIsGrounded()
        {
            isGrounded = CalculateIsGrounded(transform.position);
        }

        private bool CalculateIsGrounded(Vector3 position)
        {
            float radius = collider.radius - skinWidth;
            float halfHeight = (0.5f * collider.height) - skinWidth;
            float radiusOffset = Mathf.Min(radius, halfHeight);
            var center = position + collider.center;
            var origin = center + ((halfHeight - radiusOffset) * -transform.up);

            int hits = Physics.SphereCastNonAlloc(
                origin,
                radius,
                Vector3.down,
                groundedHits,
                skinWidth + GROUND_BUFFER,
                groundLayerMask,
                QueryTriggerInteraction.Ignore
            );

            if (hits == 0)
                return false;

            for (int i = 0; i < hits; i++)
            {
                var hit = groundedHits[i];

                if (hit.collider == collider)
                    continue;

                var normal = hit.normal;
                var angle = Vector3.Angle(normal, Vector3.up);

                if (angle <= maxSlopeAngle)
                    return true;
            }

            return false;
        }

        private bool TryGetGroundSnap(Vector3 position, out Vector3 snap)
        {
            if (velocity.y > 0)
            {
                snap = Vector3.zero;
                return false;
            }

            float radius = collider.radius - skinWidth;
            float halfHeight = (0.5f * collider.height) - skinWidth;
            float radiusOffset = Mathf.Min(radius, halfHeight);
            var center = position + collider.center;
            var origin = center + ((halfHeight - radiusOffset) * -transform.up);

            if (
                !Physics.SphereCast(
                    origin,
                    radius,
                    Vector3.down,
                    out var hit,
                    maxSnapDistance,
                    groundLayerMask,
                    QueryTriggerInteraction.Ignore
                )
            )
            {
                snap = Vector3.zero;
                return false;
            }

            snap = Mathf.Max(0.0f, hit.distance - skinWidth) * Vector3.down;
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta.With(a: 0.5f);

            switch (collider)
            {
                case CapsuleCollider capsule:
                    float halfHeight = (0.5f * capsule.height) - skinWidth;
                    float radius = capsule.radius - skinWidth;
                    float radiusOffset = Mathf.Min(radius, halfHeight);
                    var point1 = capsule.center.With(
                        y: capsule.center.y - (halfHeight - radiusOffset)
                    );
                    var point2 = capsule.center.With(
                        y: capsule.center.y + (halfHeight - radiusOffset)
                    );
                    point1 = transform.TransformPoint(point1);
                    point2 = transform.TransformPoint(point2);

                    GizmosUtil.DrawWireCapsule(point1, point2, radius);
                    break;
            }
        }
    }
}
