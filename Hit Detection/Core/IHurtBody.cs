using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHurtBody
    {
        public IHitReceiver Receiver { get; }

        public void Hit(IHitData data);
    }
}
