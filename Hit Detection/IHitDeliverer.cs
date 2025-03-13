using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Combat.HitDetection
{
    public interface IHitDeliverer
    {
        public Transform Transform { get; }

        public void OnHitDelivered(HitData hitData);
    }
}