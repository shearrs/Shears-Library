using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitResultGroup
    {
        public ReadOnlySpan<HitResult> Results { get; }
    }
}
