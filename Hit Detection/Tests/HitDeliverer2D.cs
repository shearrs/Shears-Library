using System;
using UnityEngine;
using UnityEngine.Events;

using HitData2D = Shears.HitDetection.HitData<Shears.HitDetection.HitResult2D>;

namespace Shears.HitDetection
{
    public class HitDeliverer2D : MonoBehaviour, IHitDeliverer<HitData2D>
    {
        [SerializeField] private UnityEvent<HitData2D> onDeliverHit;

        private Vector3 hitPoint;

        Transform IHitDeliverer.Transform => transform;

        public event Action<HitData2D> OnDeliverHit;

        public void OnHitDelivered(HitData2D hitData)
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
