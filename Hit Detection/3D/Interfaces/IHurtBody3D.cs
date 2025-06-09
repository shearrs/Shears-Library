using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHurtBody3D : IHurtBody<HitData3D>
    {
        new public IHitReceiver3D Receiver { get; }
    }
}
