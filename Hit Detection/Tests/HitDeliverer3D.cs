using UnityEngine;
using UnityEngine.Events;

namespace Shears.HitDetection
{
    public class HitDeliverer3D : MonoBehaviour, IHitDeliverer3D
    {
        [SerializeField] private UnityEvent<HitData3D> onDeliverHit;

        private Vector3 hitPoint;

        Transform IHitDeliverer.Transform => transform;

        public event System.Action<HitData3D> OnDeliverHit;

        public void OnHitDelivered(HitData3D hitData)
        {
            hitPoint = hitData.Result.Point;

            OnDeliverHit?.Invoke(hitData);
            onDeliverHit.Invoke(hitData);
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
