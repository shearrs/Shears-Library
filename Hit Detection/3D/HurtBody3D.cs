using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody3D : MonoBehaviour, IHurtBody
    {
        [SerializeField] private Collider col;

        public IHitReceiver Receiver { get; private set; }
        public Collider Collider => col;

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