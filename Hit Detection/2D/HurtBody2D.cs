using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody2D : MonoBehaviour, IHurtBody
    {
        [SerializeField] private Collider2D col;

        public IHitReceiver Receiver { get; private set; }
        public Collider2D Collider => col;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver>();
        }

        public void Hit(IHitData hitData)
        {
            Receiver.OnHitReceived(hitData);
        }
    }
}
