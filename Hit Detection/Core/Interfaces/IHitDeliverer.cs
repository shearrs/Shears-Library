using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitDeliverer
    {
        public Transform Transform { get; }

        public IReadOnlyCollection<IHitSubdata> GetCustomData()
        {
            return null;
        }
    }

    public interface IHitDeliverer<T>  : IHitDeliverer where T : IHitData
    {
        public void OnHitDelivered(T hitData);

        public void OnHitBlocked(T hitData)
        {

        }
    }
}