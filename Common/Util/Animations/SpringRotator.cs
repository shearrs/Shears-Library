using System;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(1000)]
    public class SpringRotator : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float springStrength = 100.0f;
        [SerializeField] private float damping = 10.0f;

        private SpringRotator waitTarget;
        private Quaternion rotation;
        private Vector3 angularVelocity;

        public event Action Updated;

        private void Awake()
        {
            targetTransform.TryGetComponent(out waitTarget);
            rotation = transform.rotation;

            if (waitTarget != null)
                waitTarget.Updated += UpdateRotationQuaternion;
        }

        private void LateUpdate()
        {
            if (waitTarget == null)
                UpdateRotationQuaternion();
        }
        
        private void UpdateRotationQuaternion()
        {
            Quaternion targetRotation = targetTransform.rotation;
            Quaternion diff = GetDifference(targetRotation, rotation);

            diff.Normalize();
            diff.ToAngleAxis(out float angle, out Vector3 axis);

            float strength = angle * springStrength;

            Vector3 force = Time.deltaTime * (strength * axis) - Time.deltaTime * (damping * angularVelocity);

            angularVelocity += force;

            float magnitude = Time.deltaTime * angularVelocity.magnitude;
            Quaternion velocityQuat = Quaternion.AngleAxis(magnitude, angularVelocity.normalized);

            rotation = velocityQuat * rotation;
            transform.rotation = rotation;

            Updated?.Invoke();
        }

        private Quaternion GetDifference(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
                return a * Quaternion.Inverse(Multiply(b, -1));
            else 
                return a * Quaternion.Inverse(b);
        }

        private Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
