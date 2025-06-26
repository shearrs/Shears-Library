using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody2D : MonoBehaviour, IHurtBody2D
    {
        [SerializeField] private Collider2D col;

        public IHitReceiver2D Receiver { get; private set; }
        public Collider2D Collider => col;

        IHitReceiver<HitData2D> IHurtBody<HitData2D>.Receiver => Receiver;

        public Bounds Bounds => col.bounds;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver2D>();
        }

        public void Hit(HitData2D hitData)
        {
            Receiver.OnHitReceived(hitData);
        }
    }
}
