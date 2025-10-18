using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shears.HitDetection
{
    public class HitDeliverer3D : MonoBehaviour, IHitDeliverer3D
    {
        Transform IHitDeliverer.Transform => transform;

        public event System.Action<HitData3D> HitDelivered;
        public event System.Action<HitData3D> HitBlocked;

        public void OnHitDelivered(HitData3D hitData)
        {
            HitDelivered?.Invoke(hitData);
        }

        public void OnHitBlocked(HitData3D hitData)
        {
            HitBlocked?.Invoke(hitData);
        }

        public virtual IReadOnlyCollection<IHitSubdata> GetCustomData()
        {
            return null;
        }
    }
}
