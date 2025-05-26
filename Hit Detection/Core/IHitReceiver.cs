using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitReceiver
    {
        public Transform Transform { get; }
    }

    public interface IHitReceiver<T> : IHitReceiver where T : IHitData
    {
        public void OnHitReceived(T hitData);
    }
}