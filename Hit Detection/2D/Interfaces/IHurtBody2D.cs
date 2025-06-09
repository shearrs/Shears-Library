using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHurtBody2D : IHurtBody<HitData2D>
    {
        new public IHitReceiver2D Receiver { get; }
        public Collider2D Collider { get; }
    }
}
