using System;
using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitDeliverer
    {
        public Transform Transform { get; }

        public object[] GetCustomData()
        {
            return Array.Empty<object>();
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