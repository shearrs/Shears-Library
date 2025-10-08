using Shears.Input;
using System;
using UnityEngine;

namespace Shears.Detection
{
    public class RayDetector3D : AreaDetector3D
    {
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

        private RaycastHit[] raycastHits;

        public bool CastCameraToCursor { get => castCameraToCursor; set => castCameraToCursor = value; }
        public Camera OriginCamera { get => originCamera; set => originCamera = value; }
        public Vector3 Offset { get => offset; set => offset = value; }
        public Vector3 Direction { get => direction; set => direction = value; }
        public float Distance { get => distance; set => distance = value; }

        protected override void Awake()
        {
            base.Awake();

            raycastHits = new RaycastHit[MaxDetections];
        }

        protected override int Sweep(Collider[] detections)
        {
            Vector3 origin;
            Vector3 dir;

            if (castCameraToCursor)
            {
                var cam = originCamera == null ? Camera.main : originCamera;

                Ray cameraRay = cam.ScreenPointToRay(ManagedPointer.Current.Position);
                origin = cameraRay.origin;
                dir = cameraRay.direction;
            }
            else
            {
                origin = transform.TransformPoint(offset);
                dir = transform.TransformDirection(direction);
            }

            int hits = Physics.RaycastNonAlloc(origin, dir, raycastHits, distance, DetectionMask, TriggerInteraction);

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
            {
                detections[i] = raycastHits[i].collider;
            }

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

            Gizmos.DrawRay(transform.position, dir * distance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, 0.15f);
        }
    }
}
