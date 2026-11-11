using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly ref struct SweepResult
    {
        private readonly List<HitResult> hits;

        public IReadOnlyList<HitResult> Hits => hits;

        public SweepResult(List<HitResult> hits)
        {
            this.hits = hits;
        }

        public void Dispose()
        {
            CollectionUtil.ReleasePooled(hits);
        }
    }
}
