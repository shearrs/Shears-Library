using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitDataProvider
    {
        public IReadOnlyCollection<IHitSubdata> GetData();
    }
}