using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitBody3D : IHitBody<HitData3D>
    {
        new public IHitDeliverer3D Deliverer { get; }
    }
}
