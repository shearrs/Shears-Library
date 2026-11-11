using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public delegate void HitEvent(in HitData data);

    public class HurtBody : MonoBehaviour
    {
        [SerializeField]
        private Collider col;

        [SerializeField]
        private bool isBlocking;

        public Collider Collider
        {
            get => col;
            set => col = value;
        }
        public bool IsBlocking
        {
            get => isBlocking;
            set => isBlocking = value;
        }

        public event HitEvent HitReceived;

        public void Hit(in HitData data)
        {
            HitReceived?.Invoke(data);
        }

        public bool CanBlock(in HitData _)
        {
            return isBlocking;
        }
    }
}
