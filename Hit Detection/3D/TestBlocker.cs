using UnityEngine;

namespace Shears.HitDetection
{
    public class TestBlocker : HitReceiver3D, IHitBlocker3D
    {
        public bool IsBlocking => true;

        public void OnHitBlocked(HitData3D hitData)
        {
            Debug.DrawRay(transform.position, Vector3.up * 2.0f, Color.blue);
        }
    }
}
