using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct DetectionHandle
    {
        private readonly LayerMask collisionMask;
        private readonly Action<RaycastHit[], int, Comparison<int>> validateCallback;

        public readonly LayerMask CollisionMask => collisionMask;
        public readonly Action<RaycastHit[], int, Comparison<int>> ValidateCallback => validateCallback;

        public DetectionHandle(LayerMask collisionMask, Action<RaycastHit[], int, Comparison<int>> validateCallback)
        {
            this.collisionMask = collisionMask;
            this.validateCallback = validateCallback;
        }
    }
}
