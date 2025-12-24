using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct DetectionHandle
    {
        public delegate void ValidationDelegate(RaycastHit[] hits, int hitCount, Comparison<int> comparison, out bool blocked);

        private readonly LayerMask collisionMask;
        private readonly ValidationDelegate validateCallback;

        public readonly LayerMask CollisionMask => collisionMask;
        public readonly ValidationDelegate ValidateCallback => validateCallback;

        public DetectionHandle(LayerMask collisionMask, ValidationDelegate validateCallback)
        {
            this.collisionMask = collisionMask;
            this.validateCallback = validateCallback;
        }
    }
}
