using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitDeliverer
    {
        public Transform Transform { get; }

        public void OnHitDelivered(IHitData hitData);
    }
}