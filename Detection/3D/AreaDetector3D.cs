using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Detection
{
    public abstract class AreaDetector3D : MonoBehaviour
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool highlightOnDetect = false;
        [SerializeField] private Color defaultGizmoColor = Color.red;
        [SerializeField, ShowIf("highlightOnDetect")] private Color detectingGizmoColor = Color.yellow;

        [Header("Detection Settings")]
        [SerializeField] protected int maxDetections = 10;
        [field: SerializeField] public LayerMask DetectionMask { get; set; } = -1;
        [field: SerializeField] public bool DetectTriggers { get; set; } = true;

        private bool isDetecting = false;
        private Color gizmoColor = Color.clear;
        private Collider[] detections;
        private int hits;

        protected QueryTriggerInteraction TriggerInteraction => (DetectTriggers)? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        public int Hits => hits;

        private void Awake()
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
            if (drawGizmos)
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
