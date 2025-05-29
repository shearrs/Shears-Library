using System;
using UnityEngine;
using UnityEngine.Events;

using HitData2D = Shears.HitDetection.HitData<Shears.HitDetection.HitResult2D>;

namespace Shears.HitDetection
{
    public class HitReceiver2D : MonoBehaviour, IHitReceiver<HitData2D>
    {
        [SerializeField] private UnityEvent<HitData2D> onReceiveHit;

        Transform IHitReceiver.Transform => transform;

        public event Action<HitData2D> OnReceiveHit;

        public void OnHitReceived(HitData2D hitData)
        {
            OnReceiveHit?.Invoke(hitData);
            onReceiveHit.Invoke(hitData);
        }
    }
}