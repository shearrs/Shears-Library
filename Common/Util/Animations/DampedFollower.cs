using UnityEngine;

namespace Shears
{
    public class DampedFollower : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform targetTransform;

        [Header("Position")]
        [SerializeField] private bool followPosition = true;
        [SerializeField] private float positionSmoothTime = 0.1f;

        [Header("Rotation")]
        [SerializeField] private bool followRotation = true;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float minimumAngleToFollow = 10f;

        private Vector3 refPosVelocity = Vector3.zero;
        private Vector3 refRotVelocity = Vector3.zero;
        private Quaternion targetRotation = Quaternion.identity;

        private bool isEnabled = true;

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        private void Update()
        {
            if (!isEnabled)
                return;

            DampedFollow();
        }

        private void DampedFollow()
        {
            if (followPosition)
                DampedPosition();

            if (followRotation)
                DampedRotation();
        }

        private void DampedPosition()
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position, ref refPosVelocity, positionSmoothTime);
        }

        private void DampedRotation()
        {
            if (targetRotation == Quaternion.identity || Quaternion.Angle(targetRotation, targetTransform.rotation) > minimumAngleToFollow)
                    targetRotation = targetTransform.rotation;

            transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, targetRotation, ref refRotVelocity, rotationSmoothTime);
        }
    }
}
