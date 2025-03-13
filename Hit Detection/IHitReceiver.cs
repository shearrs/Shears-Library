using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Combat.HitDetection
{
    public interface IHitReceiver
    {
        public Transform Transform { get; }

        public void OnHitReceived(HitData hitData);
    }

}