using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public class HurtBody : ShearsBehaviour
    {
        [Header("Hurt Body")]
        [SerializeField]
        private List<Collider> colliders = new();

        [SerializeField]
        private IBlockProvider blockProvider;

        public IReadOnlyList<Collider> Colliders => colliders;

        public event HitEvent HitReceived;
        public event HitEvent HitBlocked;

        public void Hit(in HitData data)
        {
            HitReceived?.Invoke(data);
        }

        public void BlockHit(in HitData data)
        {
            HitBlocked?.Invoke(data);
        }

        public bool CanBlock(in HitData data)
        {
            return blockProvider != null && blockProvider.CanBlock(data);
        }
    }
}
