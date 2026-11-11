using UnityEngine;

namespace Shears.HitDetection
{
    public abstract class HitShape : MonoBehaviour
    {
        protected const string DRAW_GIZMOS_FLAG = nameof(drawGizmos);

        [Header("Gizmos")]
        [SerializeField]
        private bool drawGizmos = true;

        protected bool ShouldDrawGizmos => drawGizmos;

        public abstract SweepResult Sweep(in SweepContext context);

        protected abstract void DrawGizmos();

        private void OnDrawGizmosSelected()
        {
            if (drawGizmos)
                DrawGizmos();
        }
    }
}
