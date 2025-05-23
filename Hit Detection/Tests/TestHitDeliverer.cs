using UnityEngine;

namespace Shears.HitDetection
{
    public class TestHitDeliverer : MonoBehaviour, IHitDeliverer
    {
        private Vector3 hitPoint;

        public Transform Transform => transform;

        public void OnHitDelivered(IHitData hitData)
        {
            hitPoint = hitData.Result.Point;
        }

        private void OnDrawGizmos()
        {
            if (hitPoint == Vector3.zero)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPoint, .025f);
        }
    }
}
