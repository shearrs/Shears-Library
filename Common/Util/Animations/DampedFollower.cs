using System;
using System.Collections;
using UnityEngine;

namespace Shears
{
    [DefaultExecutionOrder(100)]
    public class DampedFollower : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform targetTransform;

        [Header("Position")]
        [SerializeField] private bool followPosition = true;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private float positionSmoothTime = 0.1f;

        [Header("Rotation")]
        [SerializeField] private bool followRotation = true;
        [SerializeField] private float rotationTime = 0.1f;

        private Vector3 previousPosition = Vector3.zero;
        private Vector3 refPosVelocity = Vector3.zero;
        private Quaternion targetRotation = Quaternion.identity;

        private Quaternion rotationStart;
        private Quaternion currentRotation;
        private float elapsedTime = 0f;
        private bool isEnabled = true;
        private DampedFollower waitTarget;

        public event Action Updated;

        private void Awake()
        {
            previousPosition = transform.position;
            currentRotation = transform.rotation;
            rotationStart = transform.rotation;
        }

        private void Start()
        {
            if (targetTransform.TryGetComponent(out waitTarget))
                waitTarget.Updated += DampedFollow;

            targetRotation = targetTransform.rotation;
        }

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        private void LateUpdate()
        {
            if (!isEnabled || waitTarget != null)
                return;

            DampedFollow();
        }

        private void DampedFollow()
        {
            if (followPosition)
                DampedPosition();

            if (followRotation)
                DampedRotation();

            Updated?.Invoke();
        }

        private void DampedPosition()
        {
            var targetPos = targetTransform.TransformPoint(offset);

            transform.position = Vector3.SmoothDamp(previousPosition, targetPos, ref refPosVelocity, positionSmoothTime);
            previousPosition = transform.position;
        }

        private void DampedRotation()
        {
            if (targetRotation != targetTransform.rotation)
            {
                elapsedTime = 0.0f;
                targetRotation = targetTransform.rotation;
                rotationStart = currentRotation;
            }
            else if (elapsedTime >= rotationTime)
            {
                transform.rotation = targetRotation;
                currentRotation = transform.rotation;

                return;
            }

            Debug.Log("slerp");
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationTime;

            transform.rotation = Quaternion.Slerp(rotationStart, targetRotation, t);
            currentRotation = transform.rotation;

            //transform.rotation = QuaternionUtil.SmoothDamp(previousRotation, targetRotation, ref refRotVelocity, rotationSmoothTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (targetTransform == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetTransform.TransformPoint(offset), 0.15f);
        }
    }
}
