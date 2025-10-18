using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitBlocker3D : IHitReceiver3D
    {
        public bool IsBlocking { get; }

        public void OnHitBlocked(HitData3D hitData);
    }
}
