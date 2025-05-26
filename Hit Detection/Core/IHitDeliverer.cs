using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitDeliverer
    {
        public Transform Transform { get; }
    }

    public interface IHitDeliverer<T>  : IHitDeliverer where T : IHitData
    {
        public void OnHitDelivered(T hitData);
    }
}