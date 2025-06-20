using System.Collections;
using UnityEngine;

namespace Shears.Detection
{
    public abstract class AreaDetector2D : MonoBehaviour
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool highlightOnDetect = false;
        [SerializeField] private Color defaultGizmoColor = Color.blue;
        [SerializeField] private Color detectingGizmoColor = Color.yellow;

        [Header("Detection Settings")]
        [SerializeField] protected int maxDetections = 10;
        [SerializeField] private LayerMask detectionMask = 1;
        [SerializeField] private bool detectTriggers = true;

        private Color gizmoColor = Color.clear;

        public LayerMask DetectionMask { get => detectionMask; set => detectionMask = value; }
        public bool DetectTriggers { get => detectTriggers; set => detectTriggers = value; }

        protected ContactFilter2D ContactFilter => new()
        {
            useTriggers = DetectTriggers,
            layerMask = DetectionMask,
            useLayerMask = true
        };

        public bool Detect(Collider2D[] detections, out int hits)
        {
            hits = Sweep(detections);

            if (highlightOnDetect)
            {
                StopAllCoroutines();
                StartCoroutine(IEHighlightGizmos());
            }

            return hits > 0;
        }

        protected abstract int Sweep(Collider2D[] detections);

        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                if (gizmoColor != defaultGizmoColor && gizmoColor != detectingGizmoColor)
                    gizmoColor = defaultGizmoColor;

                Gizmos.color = gizmoColor;
                DrawWireGizmos();
            }
        }

        protected abstract void DrawWireGizmos();

        private IEnumerator IEHighlightGizmos()
        {
            gizmoColor = detectingGizmoColor;

            yield return CoroutineUtil.WaitForSeconds(0.1f);

            gizmoColor = defaultGizmoColor;
        }
    }
}
