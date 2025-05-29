using UnityEngine;

using HitData2D = Shears.HitDetection.HitData<Shears.HitDetection.HitResult2D>;

namespace Shears.HitDetection
{
    public class HurtBody2D : MonoBehaviour, IHurtBody<HitData2D>
    {
        [SerializeField] private Collider2D col;

        public IHitReceiver<HitData2D> Receiver { get; private set; }
        public Collider2D Collider => col;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver<HitData2D>>();
        }

        public void Hit(HitData2D hitData)
        {
            Receiver.OnHitReceived(hitData);
        }
    }
}
