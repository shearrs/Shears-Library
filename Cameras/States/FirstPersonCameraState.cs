using Shears.Input;
using UnityEngine;

namespace ShearsLibrary.Cameras
{
    public class FirstPersonCameraState : CameraState
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;

        [Header("Rotation Settings")]
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float minXRotation = -89f;
        [SerializeField] private float maxXRotation = 89f;

        private IManagedInput lookInput;

        private Vector3 TargetPosition => target.TransformPoint(offset);

        public override void Initialize()
        {
            lookInput = InputProvider.GetInput("Look");
        }

        protected override void OnEnter()
        {
            CursorManager.SetCursorLockMode(CursorLockMode.Locked);
            CursorManager.SetCursorVisibility(false);
        }

        protected override void OnExit()
        {
        }

        protected override void OnUpdate()
        {
            CameraTransform.position = TargetPosition;

            UpdateRotation(lookInput.ReadValue<Vector2>());
        }

        private void UpdateRotation(Vector2 input)
        {
            var forward = CameraTransform.forward;
            forward.y = 0;

            var currentXRotation = Vector3.SignedAngle(forward, CameraTransform.forward, CameraTransform.right);
            var currentYRotation = CameraTransform.rotation.eulerAngles.y;

            var newXRotation = currentXRotation - input.y * sensitivity;
            var newYRotation = currentYRotation + input.x * sensitivity;

            newXRotation = Mathf.Clamp(newXRotation, minXRotation, maxXRotation);

            CameraTransform.rotation = Quaternion.Euler(newXRotation, newYRotation, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(target.position, TargetPosition);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(TargetPosition, 0.2f);
            }
        }
    }
}
