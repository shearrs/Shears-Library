using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitData
    {
        public IHitDeliverer Deliverer { get; }
        public IHitReceiver Receiver { get; }
        public IHitBody HitBody { get; }
        public IHurtBody HurtBody { get; }
        public IHitResult Result { get; }
    }
}
