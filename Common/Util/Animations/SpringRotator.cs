using System;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(1000)]
    public class SpringRotator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool fixedUpdate = true;
        [SerializeField] private float strength = 5000.0f;
        [SerializeField] private float damping = 10.0f;

        [Header("Constraints")]
        [SerializeField] private bool clampZToZero = false;

        private SpringRotator waitTarget;
        private Quaternion rotation;
        private Vector3 angularVelocity;

        internal Quaternion Rotation => rotation;

        public event Action Updated;

        private void Awake()
        {
            rotation = transform.rotation;

            if (targetTransform.TryGetComponent(out waitTarget))
                waitTarget.Updated += TestRotate;
        }

        private void LateUpdate()
        {
            if (fixedUpdate)
                return;

            if (waitTarget == null)
                TestRotate();
        }

        private void FixedUpdate()
        {
            if (!fixedUpdate)
                return;

            if (waitTarget == null)
                TestRotate();
        }
        
        //private void LateUpdate()
        //{
        //    transform.rotation = rotation;
        //}

        private void TestRotate()
        {
            Quaternion targetRotation;

            if (waitTarget != null)
                targetRotation = waitTarget.Rotation;
            else
                targetRotation = targetTransform.rotation;

            Quaternion deltaRotation = ShortestRotation(rotation, targetRotation);

            Vector3 angularDisplacement = Vector3.ClampMagnitude(AngularError(deltaRotation), 45f * Mathf.Deg2Rad);
            Vector3 torque = -((strength * angularDisplacement) + (damping * angularVelocity));

            angularVelocity += torque * Time.deltaTime;

            float magnitude = angularVelocity.magnitude;

            if (Mathf.Approximately(magnitude, 0.0f))
                return;

            Quaternion angularVelocityQuat = Quaternion.AngleAxis(Time.deltaTime * magnitude, angularVelocity / magnitude);
            rotation = angularVelocityQuat * rotation;

            transform.rotation = rotation;

            float zAngle = transform.localEulerAngles.z;
            if (zAngle > 180f)
                zAngle -= 360f;

            if (clampZToZero && zAngle > 0.0f)
            {
                transform.localEulerAngles = transform.localEulerAngles.With(z: 0.0f);
                rotation = transform.rotation;
            }

            Updated?.Invoke();
        }

        //private void UpdateRotationQuaternion()
        //{
        //    Quaternion targetRotation = targetTransform.rotation;
        //    Quaternion diff = GetDifference(targetRotation, rotation);

        //    diff.Normalize();
        //    diff.ToAngleAxis(out float angle, out Vector3 axis);

        //    float strength = angle * springStrength;
        //    Vector3 force = (strength * axis) - (damping * angularVelocity);

        //    angularVelocity += Time.deltaTime * force;

        //    float magnitude = Time.deltaTime * angularVelocity.magnitude;
        //    Quaternion velocityQuat = Quaternion.AngleAxis(magnitude, angularVelocity.normalized);

        //    rotation = velocityQuat * rotation;
        //    transform.rotation = rotation;

        //    Updated?.Invoke();
        //}

        public static Vector3 AngularError(Quaternion q)
        {
            if (q.w < 0)
                q = Multiply(q, -1);

            Vector3 v = new(q.x, q.y, q.z);
            return 2f * v;
        }

        public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }

            else return a * Quaternion.Inverse(b);
        }

        public static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
