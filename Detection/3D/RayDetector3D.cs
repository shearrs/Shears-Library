using Shears.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public class RayDetector3D : AreaDetector3D
    {
        [SerializeField, Tooltip("Continous detection can solve missing detections at lower framerates or higher speeds.")]
        private bool continuousDetection = false;

        [SerializeField, ShowIf("continuousDetection"), Min(0.01f)]
        private float continuousDetectionStep = 0.1f;

        [Header("Ray Settings")]
        [SerializeField, Tooltip("Whether or not to cast from the camera's world position to the cursor's world position.")]
        private bool castCameraToCursor = false;

        [SerializeField, ShowIf("castCameraToCursor"), Tooltip("The camera to cast from. If null, defaults to the MainCamera.")]
        private Camera originCamera;

        [SerializeField, ShowIf("!castCameraToCursor"), Tooltip("The local offset for the raycast's origin.")] 
        private Vector3 offset = Vector3.zero;

        [SerializeField, ShowIf("!castCameraToCursor"), Tooltip("The local direction to raycast in.")] 
        private Vector3 direction = Vector3.forward;

        [SerializeField, Tooltip("The distance to cast.")]
        private float distance = 1.0f;

        private readonly List<Collider> continuousDetections = new();
        private Collider[] singleContinuousDetection;
        private RaycastHit[] raycastHits;
        private bool isRayDetecting = false;
        private bool isFirstFrame = true;
        private Vector3 previousPosition;

        public bool CastCameraToCursor { get => castCameraToCursor; set => castCameraToCursor = value; }
        public Camera OriginCamera { get => originCamera; set => originCamera = value; }
        public Vector3 Offset { get => offset; set => offset = value; }
        public Vector3 Direction { get => direction; set => direction = value; }
        public float Distance { get => distance; set => distance = value; }

        protected override void Awake()
        {
            base.Awake();

            raycastHits = new RaycastHit[MaxDetections];
            singleContinuousDetection = new Collider[MaxDetections];
        }

        private void Update()
        {
            if (isRayDetecting)
                isRayDetecting = false;
            else
                isFirstFrame = true;

            previousPosition = transform.TransformPoint(offset);
        }

        protected override int Sweep(Collider[] detections)
        {
            isRayDetecting = true;

            if (castCameraToCursor)
                return CameraSweep(detections);
            else
            {
                if (continuousDetection && !isFirstFrame)
                    return ContinuousSweep(detections);
                else
                {
                    isFirstFrame = false;
                    return Sweep(transform.TransformPoint(offset), transform.TransformDirection(direction), detections);
                }
            }
        }

        private int CameraSweep(Collider[] detections)
        {
            var cam = originCamera == null ? Camera.main : originCamera;

            Ray cameraRay = cam.ScreenPointToRay(ManagedPointer.Current.Position);
            Vector3 origin = cameraRay.origin;
            Vector3 dir = cameraRay.direction;

            return Sweep(origin, dir, detections);
        }

        private int ContinuousSweep(Collider[] detections)
        {
            continuousDetections.Clear();

            Vector3 currentPos = transform.TransformPoint(offset);
            Vector3 direction = transform.TransformDirection(this.direction);

            Vector3 heading = currentPos - previousPosition;
            float distance = heading.magnitude;
            Vector3 offsetDirection = heading / distance;

            if (distance < continuousDetectionStep)
                return Sweep(currentPos, direction, detections);

            float traveled = 0f;

            while (traveled <= distance)
            {
                Vector3 currentOrigin = previousPosition + offsetDirection * traveled;

                int hits = Sweep(currentOrigin, direction, singleContinuousDetection);

                for (int i = 0; i < hits; i++)
                {
                    var detection = singleContinuousDetection[i];

                    continuousDetections.Add(detection);
                }

                if (traveled == distance)
                    break;

                traveled = Mathf.Min(traveled + continuousDetectionStep, distance);
            }

            for (int i = 0; i < detections.Length; i++)
            {
                if (i >= continuousDetections.Count)
                    break;

                detections[i] = continuousDetections[i];
            }

            return continuousDetections.Count;
        }

        private int Sweep(Vector3 origin, Vector3 direction, Collider[] detections)
        {
            int hits = Physics.RaycastNonAlloc(origin, direction, raycastHits, distance, DetectionMask, TriggerInteraction);

            Array.Sort(raycastHits, (h1, h2) =>
            {
                if (h1.collider == null)
                {
                    if (h2.collider == null)
                        return 0;
                    else
                        return 1;
                }
                else if (h2.collider == null)
                {
                    if (h1.collider == null)
                        return 0;
                    else
                        return -1;
                }
                else
                    return h1.distance.CompareTo(h2.distance);
            });

            if (hits > MaxDetections)
                hits = MaxDetections;

            for (int i = 0; i < hits; i++)
                detections[i] = raycastHits[i].collider;

            return hits;
        }

        public RaycastHit GetHit(int index) => raycastHits[index];
        public RaycastHit GetHit(Collider collider)
        {
            foreach (var hit in raycastHits)
            {
                if (hit.collider == collider)
                    return hit;
            }

            return default;
        }

        protected override void DrawWireGizmos()
        {
            Vector3 origin;
            Vector3 dir;

            if (castCameraToCursor && Application.isPlaying)
            {
                var cam = originCamera == null ? Camera.main : originCamera;
                Vector2 pointerPosition = ManagedPointer.Current.Position;
                pointerPosition.y *= -1;

                Ray cameraRay = cam.ScreenPointToRay(pointerPosition);
                origin = cameraRay.origin;
                dir = cameraRay.direction;
            }
            else
            {
                origin = transform.TransformPoint(offset);
                dir = transform.TransformDirection(direction);
            }

            Gizmos.DrawRay(origin, dir * distance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, 0.15f);
        }
    }
}
