using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody3D : MonoBehaviour, IHurtBody<HitData3D>
    {
        [SerializeField] private Collider col;

        public IHitReceiver<HitData3D> Receiver { get; private set; }
        public Collider Collider => col;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver<HitData3D>>();
        }
    }
}