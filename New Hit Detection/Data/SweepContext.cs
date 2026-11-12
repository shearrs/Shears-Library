using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly ref struct SweepContext
    {
        public LayerMask CollisionMask { get; }
        public bool IsUnblockable { get; }

        public SweepContext(LayerMask collisionMask, bool isUnblockable)
        {
            CollisionMask = collisionMask;
            IsUnblockable = isUnblockable;
        }
    }
}
