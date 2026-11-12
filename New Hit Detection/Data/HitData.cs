using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly ref struct HitData
    {
        public HitShape SourceShape { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public ReadOnlySpan<HitSubdata> Subdata { get; }

        public HitData(
            HitShape sourceShape,
            Vector3 hitPoint,
            Vector3 hitNormal,
            ReadOnlySpan<HitSubdata> subdata
        )
        {
            SourceShape = sourceShape;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Subdata = subdata;
        }

        public bool TryGetData<T>(out T data)
            where T : HitSubdata
        {
            foreach (ref readonly var subdata in Subdata)
            {
                if (subdata is T typeData)
                {
                    data = typeData;
                    return true;
                }
            }

            data = default;
            return false;
        }
    }
}
