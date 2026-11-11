using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly ref struct HitData
    {
        public HitShape SourceShape { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public ReadOnlySpan<HitSubdata> Subdata { get; }
    }
}
