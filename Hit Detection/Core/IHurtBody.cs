using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHurtBody
    {
    }

    public interface IHurtBody<T> : IHurtBody where T : IHitData
    {
        public IHitReceiver<T> Receiver { get; }
    }
}
