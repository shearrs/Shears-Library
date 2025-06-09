using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody3D : MonoBehaviour, IHurtBody3D
    {
        [SerializeField] private Collider col;

        public IHitReceiver3D Receiver { get; private set; }
        public Collider Collider => col;

        IHitReceiver<HitData3D> IHurtBody<HitData3D>.Receiver => Receiver;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver3D>();
        }
    }
}