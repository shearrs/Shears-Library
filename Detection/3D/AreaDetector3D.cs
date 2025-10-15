using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public abstract class AreaDetector3D : MonoBehaviour
    {
        protected enum GizmoType { Draw, OnSelected, DontDraw };

        [FoldoutGroup("Gizmos", 4)]
        [SerializeField, Tooltip("Whether or not to draw gizmos.")] 
        protected GizmoType gizmoType = GizmoType.OnSelected;

        [SerializeField, Tooltip("Whether or not to highlight gizmos when actively detecting.")] 
        private bool highlightOnDetect = false;

        [SerializeField, Tooltip("The default gizmo color.")] 
        private Color defaultGizmoColor = Color.red;

        [SerializeField, Tooltip("The gizmo color when actively detecting.")] 
        private Color detectingGizmoColor = Color.yellow;

        [Header("Detection Settings")]
        [SerializeField, Tooltip("The maximum number of detections for one detection. Does not update at runtime.")]
        protected int maxDetections = 10;

        [SerializeField, Tooltip("The layermask to detect colliders on.")] 
        private LayerMask detectionMask = -1;

        [SerializeField, Tooltip("Whether or not trigger colliders can be detected.")] 
        private bool detectTriggers = true;

        private bool isDetecting = false;
        private Color gizmoColor = Color.clear;
        private Collider[] detections;
        private int hits;

        protected int MaxDetections => maxDetections;
        protected QueryTriggerInteraction TriggerInteraction => (DetectTriggers)? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
        public LayerMask DetectionMask { get => detectionMask; set => detectionMask = value; }
        public bool DetectTriggers { get => detectTriggers; set => detectTriggers = value; }
        public int Hits => hits;

        protected virtual void Awake()
        {
            detections = new Collider[maxDetections];
        }

        public bool Detect()
        {
            hits = Sweep(detections);

            if (highlightOnDetect)
            {
                StopAllCoroutines();
                StartCoroutine(IEHighlightGizmos());
            }

            return hits > 0;
        }

        public Collider GetDetection(int index) => detections[index];

        public bool TryGetDetection<T>(out T component, bool checkParents = false)
        {
            component = default;

            for (int i = 0; i < hits; i++)
            {
                if (detections[i].TryGetComponent(out component))
                    return true;
                else if (checkParents)
                {
                    component = detections[i].GetComponentInParent<T>();

                    if (component != null)
                        return true;
                }
            }

            return false;
        }

        public IReadOnlyCollection<Collider> GetDetections()
        {
            return detections;
        }

        protected abstract int Sweep(Collider[] detections);

        private void OnDrawGizmos()
        {
            if (gizmoType == GizmoType.Draw)
            {
                if (gizmoColor == Color.clear)
                    gizmoColor = defaultGizmoColor;
                else if (!isDetecting)
                    gizmoColor = defaultGizmoColor;

                Gizmos.color = gizmoColor;
                DrawWireGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (gizmoType == GizmoType.OnSelected)
            {
                if (gizmoColor == Color.clear)
                    gizmoColor = defaultGizmoColor;
                else if (!isDetecting)
                    gizmoColor = defaultGizmoColor;

                Gizmos.color = gizmoColor;
                DrawWireGizmos();
            }
        }

        protected abstract void DrawWireGizmos();

        private IEnumerator IEHighlightGizmos()
        {
            isDetecting = true;
            gizmoColor = detectingGizmoColor;

            yield return CoroutineUtil.WaitForSeconds(0.1f);

            gizmoColor = defaultGizmoColor;
            isDetecting = false;
        }
    }
}
