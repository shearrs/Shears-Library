using System;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.HitDetection
{
    public class HitReceiver3D : MonoBehaviour, IHitReceiver3D
    {
        Transform IHitReceiver.Transform => transform;

        public event Action<HitData3D> ReceivedHit;

        public void OnHitReceived(HitData3D hitData)
        {
            ReceiveHit(hitData);
            ReceivedHit?.Invoke(hitData);
        }

        protected virtual void ReceiveHit(HitData3D hitData) { }
    }
}
