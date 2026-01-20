using System;
using System.Collections;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(1000)]
    public class SpringRotator : MonoBehaviour
    {
        private const float PRECISION_CUTOFF = 0.005f;

        [Header("Spring Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float strength = 5000.0f;
        [SerializeField] private float damping = 10.0f;

        [Header("Update Settings")]
        [SerializeField] private bool usesFixedUpdate = true;
        [SerializeField, ShowIf(nameof(usesFixedUpdate)), RuntimeReadOnly] private bool usesCustomFixedTimestep = false;
        [SerializeField, ShowIf(nameof(usesCustomFixedTimestep)), Min(0.001f)] private float timestep = 0.01f;

        [Header("Constraints")]
        [SerializeField] private Range<float> xRange = new(-180f, 180f);
        [SerializeField] private Range<float> yRange = new(-180f, 180f);
        [SerializeField] private Range<float> zRange = new(-180f, 180f);

        private SpringRotator waitTarget;
        private Quaternion rotation;
        private Vector3 angularVelocity;
        private float velocityMagnitude;
        private float angularErrorSqrMagnitude;

        internal Quaternion Rotation => rotation;
        public Vector3 Offset { get => offset; set => offset = value; }
        public float Strength { get => strength; set => strength = value; }
        public float Damping { get => damping; set => damping = value; }
        public Range<float> XRange { get => xRange; set => xRange = value; }
        public Range<float> YRange { get => yRange; set => yRange = value; }
        public Range<float> ZRange { get => zRange; set => zRange = value; }
        public float VelocityMagnitude => velocityMagnitude;
        public float AngularErrorSqrMagnitude => angularErrorSqrMagnitude;
        public bool IsAtTarget => velocityMagnitude < PRECISION_CUTOFF && angularErrorSqrMagnitude < PRECISION_CUTOFF * PRECISION_CUTOFF;

        public event Action Updated;

        private void Start()
        {
            rotation = transform.rotation;

            if (targetTransform.TryGetComponent(out waitTarget))
                waitTarget.Updated += SpringToTarget;
        }

        private void OnEnable()
        {
            if (usesCustomFixedTimestep && waitTarget == null)
                StartCoroutine(IECustomFixedUpdate());
        }

        private void LateUpdate()
        {
            if (usesFixedUpdate || usesCustomFixedTimestep)
                return;

            if (waitTarget == null)
                SpringToTarget();
        }

        private void FixedUpdate()
        {
            if (!usesFixedUpdate || usesCustomFixedTimestep)
                return;

            if (waitTarget == null)
                SpringToTarget();
        }

        private IEnumerator IECustomFixedUpdate()
        {
            while (true)
            {
                yield return CoroutineUtil.WaitForSeconds(timestep);

                SpringToTarget();
            }
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

        public bool IsWithinPrecision(float velocityPrecision, float anglePrecision) => velocityMagnitude < velocityPrecision && angularErrorSqrMagnitude < anglePrecision * anglePrecision;

        private void SpringToTarget()
        {
            Quaternion targetRotation;
            float deltaTime = usesCustomFixedTimestep ? timestep : Time.deltaTime;

            if (waitTarget != null)
                targetRotation = waitTarget.Rotation;
            else
                targetRotation = targetTransform.rotation;

            targetRotation = Quaternion.Euler(targetTransform.TransformDirection(offset)) * targetRotation;

            Quaternion deltaRotation = ShortestRotation(rotation, targetRotation);
            Vector3 angularError = AngularError(deltaRotation);
            Vector3 angularDisplacement = Vector3.ClampMagnitude(angularError, 45f * Mathf.Deg2Rad);
            Vector3 torque = -((strength * angularDisplacement) + (damping * angularVelocity));

            angularVelocity += torque * deltaTime;
            velocityMagnitude = angularVelocity.magnitude;
            angularErrorSqrMagnitude = angularError.sqrMagnitude;

            if (velocityMagnitude < PRECISION_CUTOFF && angularErrorSqrMagnitude < PRECISION_CUTOFF * PRECISION_CUTOFF)
            {
                rotation = targetRotation;
                transform.rotation = rotation;
                angularVelocity = Vector3.zero;

                Updated?.Invoke();
                
                return;
            }

            Quaternion angularVelocityQuat = Quaternion.AngleAxis(deltaTime * velocityMagnitude, angularVelocity / velocityMagnitude);
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
