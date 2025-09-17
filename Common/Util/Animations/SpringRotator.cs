using System;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(1000)]
    public class SpringRotator : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float springStrength = 1.0f;
        [SerializeField] private float damping = 0.5f;

        private SpringRotator waitTarget;
        private Quaternion rotation;
        private Vector3 angularVelocity;

        public event Action Updated;

        private void Awake()
        {
            targetTransform.TryGetComponent(out waitTarget);
        }

        private void Update()
        {
            if (waitTarget == null)
                UpdateRotation();
        }

        // try doing this with only one axis
        private void UpdateRotation()
        {
            Quaternion targetRotation = targetTransform.rotation;
            Quaternion offset = Quaternion.Inverse(rotation) * targetRotation;
            offset.ToAngleAxis(out float angle, out Vector3 axis);

            float velocity = Vector3.Dot(axis, angularVelocity);
            float force = (angle * springStrength) - (velocity * damping);

            angularVelocity += force * axis;
            transform.eulerAngles += angularVelocity;
            rotation = transform.rotation;
        }
    }
}
