using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitBody2D : IHitBody<HitData2D>
    {
        new public IHitDeliverer2D Deliverer { get; }
    }
}
