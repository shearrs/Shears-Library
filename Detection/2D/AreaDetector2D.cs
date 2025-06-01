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
        [field: SerializeField] public LayerMask DetectionMask { get; set; } = 1;
        [field: SerializeField] public bool DetectTriggers { get; set; } = true;

        private Color gizmoColor = Color.clear;

        protected ContactFilter2D ContactFilter => new()
        {
            useTriggers = DetectTriggers,
            layerMask = DetectionMask
        };

        public int Detect(Collider2D[] detections)
        {
            int hits = Sweep(detections);

            if (highlightOnDetect)
            {
                StopAllCoroutines();
                StartCoroutine(IEHighlightGizmos());
            }

            return hits;
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
