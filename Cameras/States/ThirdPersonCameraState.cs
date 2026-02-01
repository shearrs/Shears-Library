using Shears.Input;
using System.Collections;
using UnityEngine;

namespace Shears.Cameras
{
    public class ThirdPersonCameraState : CameraState
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 lookAtOffset;

        [Header("Movement Settings")]
        [SerializeField] private float sensitivity = 0.2f;
        [SerializeField, Min(0)] private float smoothing = 25f;
        [SerializeField, Range(-89f, 89f)] private float minXRotation = -89f;
        [SerializeField, Range(-89f, 89f)] private float maxXRotation = 89f;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSensitivity = 1f;
        [SerializeField, Min(0)] private float zoomTime = 0.1f;
        [SerializeField, Delayed, Min(0)] private float minZoom = 4f;
        [SerializeField, Delayed, Min(0)] private float maxZoom = 16f;

        [Header("Occlusion Settings")]
        [SerializeField] private bool occlusionEnabled = true;
        [SerializeField] private float occlusionMoveSpeed = 1.0f;
        [SerializeField, ShowIf("occlusionEnabled")] private LayerMask occlusionLayers = 1;
        [SerializeField, ShowIf("occlusionEnabled"), Min(0)] private float occlusionRadius = 0.5f;
        [SerializeField, ShowIf("occlusionEnabled"), Min(0)] private float occlusionPadding = 0.1f;

        private readonly CoroutineChain zoomChain = new();
        private Vector3 targetPosition;
        private Vector2 orbit;
        private float zoom;
        private float targetZoom;
        private float targetDistance;

        private IManagedInput lookInput;
        private IManagedInput zoomInput;

        private Vector3 FocusPosition => target.TransformPoint(lookAtOffset);

        public float Smoothing { get => smoothing; set => smoothing = value; } 
        public float Zoom { get => zoom; set => zoom = value; }
        public float MinZoom { get => minZoom; set => minZoom = value; }
        public float MaxZoom { get => maxZoom; set => maxZoom = value; }

        private void OnValidate()
        {
            if (target == null)
                return;

            float zoom = 0.5f * (minZoom + maxZoom);
            Vector3 position = FocusPosition - Vector3.forward * zoom;
            var rotation = Quaternion.LookRotation(FocusPosition - position);

            transform.SetPositionAndRotation(position, rotation);

            if (maxZoom < minZoom)
                maxZoom = minZoom;

            if (maxXRotation < minXRotation)
                maxXRotation = minXRotation;
        }

        public override void Initialize()
        {
            lookInput = InputProvider.GetInput("Look");
            zoomInput = InputProvider.GetInput("Zoom");

            zoom = 0.5f * (minZoom + maxZoom);
            targetZoom = zoom;
            targetDistance = zoom;
        }

        protected override void OnEnter()
        {
            CursorManager.SetCursorLockMode(CursorLockMode.Locked);
            CursorManager.SetCursorVisibility(false);
        }

        protected override void OnExit()
        {
        }

        protected override void OnLateUpdate()
        {
            UpdateZoom();
            UpdateTargetPosition();
            UpdatePosition();
            UpdateRotation();
        }

        protected override void OnFixedUpdate()
        {
            if (occlusionEnabled)
                UpdateOcclusion();
            else
                targetDistance = zoom;
        }

        private void UpdateOcclusion()
        {
            Vector3 direction = (targetPosition - FocusPosition).normalized;

            if (Physics.SphereCast(FocusPosition, occlusionRadius, direction, out var hit, zoom, occlusionLayers))
            {
                float distance = (hit.distance - occlusionPadding >= 0) ? hit.distance - occlusionPadding : 0.1f;

                targetDistance = Mathf.MoveTowards(targetDistance, distance, occlusionMoveSpeed * Time.fixedDeltaTime);
            }
            else
                targetDistance = zoom;
        }

        private void UpdateZoom()
        {
            float scroll = zoomInput.ReadValue<Vector2>().y;

            if (Mathf.Abs(scroll) < 0.1f)
                return;

            targetZoom -= zoomSensitivity * scroll;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            float from = zoom;
            float to = targetZoom;

            zoomChain.Stop();
            zoomChain.Clear();
            zoomChain
                .Tween((t) => zoom = Mathf.Lerp(from, to, t), zoomTime)
                .Start();
        }

        private void UpdateTargetPosition()
        {
            Vector2 input = lookInput.ReadValue<Vector2>();

            orbit += sensitivity * new Vector2(-input.y, input.x);
            orbit.x = Mathf.Clamp(orbit.x, minXRotation, maxXRotation);

            if (orbit.y < 0f)
                orbit.y += 360f;
            else if (orbit.y >= 360f)
                orbit.y -= 360f;

            Vector3 direction = Quaternion.Euler(orbit) * Vector3.forward;

            targetPosition = FocusPosition - direction * targetDistance;
        }

        private void UpdateRotation()
        {
            var rotation = Quaternion.LookRotation(FocusPosition - transform.position, Vector3.up);

            transform.rotation = rotation;
        }

        private void UpdatePosition()
        {
            transform.position = Vector3.Slerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(FocusPosition, 0.35f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(FocusPosition, minZoom);
            Gizmos.DrawWireSphere(FocusPosition, maxZoom);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(FocusPosition, zoom);
            }
        }
    }
}
