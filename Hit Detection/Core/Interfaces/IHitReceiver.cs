using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitReceiver
    {
        public Transform Transform { get; }

        public void OnHitReceived(IHitData hitData);
    }

}