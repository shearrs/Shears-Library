using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitBody
    {
        public int ValidHitCount { get; }
    }

    public interface IHitBody<T> : IHitBody where T : IHitData
    {
        public IHitDeliverer<T> Deliverer { get; }
    }
}