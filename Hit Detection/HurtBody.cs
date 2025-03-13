using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Combat.HitDetection
{
    public class HurtBody : MonoBehaviour
    {
        [SerializeField] private Collider col;

        public IHitReceiver Receiver { get; private set; }
        public Collider Collider => col;

        private void Start()
        {
            Receiver = GetComponentInParent<IHitReceiver>();
        }

        public void Hit(HitData hitData)
        {
            Receiver.OnHitReceived(hitData);
        }
    }
}