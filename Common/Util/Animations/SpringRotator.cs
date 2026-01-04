using System;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(1000)]
    public class SpringRotator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private bool fixedUpdate = true;
        [SerializeField] private float strength = 5000.0f;
        [SerializeField] private float damping = 10.0f;

        [Header("Constraints")]
        [SerializeField] private Range<float> xRange = new(-180f, 180f);
        [SerializeField] private Range<float> yRange = new(-180f, 180f);
        [SerializeField] private Range<float> zRange = new(-180f, 180f);

        private SpringRotator waitTarget;
        private Quaternion rotation;
        private Vector3 angularVelocity;

        internal Quaternion Rotation => rotation;
        public float Strength { get => strength; set => strength = value; }
        public float Damping { get => damping; set => damping = value; }
        public Range<float> XRange { get => xRange; set => xRange = value; }
        public Range<float> YRange { get => yRange; set => yRange = value; }
        public Range<float> ZRange { get => zRange; set => zRange = value; }

        public event Action Updated;

        private void Start()
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

        public void SyncRotation()
        {
            Quaternion targetRotation;

            if (waitTarget != null)
                targetRotation = waitTarget.Rotation;
            else
                targetRotation = targetTransform.rotation;

            targetRotation = Quaternion.Euler(targetTransform.TransformDirection(offset)) * targetRotation;

            rotation = targetRotation;
            transform.rotation = rotation;
        }

        private void TestRotate()
        {
            Quaternion targetRotation;

            if (waitTarget != null)
                targetRotation = waitTarget.Rotation;
            else
                targetRotation = targetTransform.rotation;

            targetRotation = Quaternion.Euler(targetTransform.TransformDirection(offset)) * targetRotation;

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

            Vector3 eulerAngles = transform.localEulerAngles.EulerMap();
            eulerAngles.x = xRange.Clamp(eulerAngles.x);
            eulerAngles.y = yRange.Clamp(eulerAngles.y);
            eulerAngles.z = zRange.Clamp(eulerAngles.z);

            transform.localEulerAngles = eulerAngles;
            rotation = transform.rotation;

            Updated?.Invoke();
        }

        private Vector3 AngularError(Quaternion q)
        {
            if (q.w < 0)
                q = Multiply(q, -1);

            Vector3 v = new(q.x, q.y, q.z);
            return 2f * v;
        }

        private Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }

            else return a * Quaternion.Inverse(b);
        }

        private Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
