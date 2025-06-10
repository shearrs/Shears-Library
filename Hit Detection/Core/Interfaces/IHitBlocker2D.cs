using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitBlocker2D : IHitReceiver2D
    {
        public bool IsBlocking { get; }
    }
}
