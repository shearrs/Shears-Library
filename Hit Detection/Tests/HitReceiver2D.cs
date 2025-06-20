using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.HitDetection
{
    public class HitReceiver2D : MonoBehaviour, IHitReceiver2D
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