using System;
using UnityEngine;
using UnityEngine.Events;

using HitData3D = Shears.HitDetection.HitData<Shears.HitDetection.HitResult3D>;

namespace Shears.HitDetection
{
    public class HitReceiver3D : MonoBehaviour, IHitReceiver<HitData3D>
    {
        [SerializeField] private UnityEvent<HitData3D> onReceiveHit;

        Transform IHitReceiver.Transform => transform;

        public event Action<HitData3D> OnReceiveHit;

        public void OnHitReceived(HitData3D hitData)
        {
            OnReceiveHit?.Invoke(hitData);
            onReceiveHit.Invoke(hitData);
        }
    }
}
