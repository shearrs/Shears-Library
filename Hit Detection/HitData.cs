using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Combat.HitDetection
{
    public struct HitData
    {
        public static HitData Empty = new(null, null, null, null, default);

        public readonly IHitDeliverer deliverer;
        public readonly IHitReceiver receiver;
        public readonly HitBody hitBody;
        public readonly HurtBody hurtBody;
        public readonly RaycastHit hit;

        public HitData(IHitDeliverer deliverer, IHitReceiver receiver, HitBody hitBody, HurtBody hurtBody, RaycastHit hit)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.hit = hit;
        }

        public override bool Equals(object obj)
        {
            return obj is HitData data &&
                   EqualityComparer<IHitDeliverer>.Default.Equals(deliverer, data.deliverer) &&
                   EqualityComparer<IHitReceiver>.Default.Equals(receiver, data.receiver) &&
                   EqualityComparer<HitBody>.Default.Equals(hitBody, data.hitBody) &&
                   EqualityComparer<HurtBody>.Default.Equals(hurtBody, data.hurtBody) &&
                   EqualityComparer<RaycastHit>.Default.Equals(hit, data.hit);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(deliverer, receiver, hitBody, hurtBody, hit);
        }

        public static bool operator==(HitData a, HitData b)
        {
            return a.deliverer == b.deliverer && a.receiver == b.receiver 
                && a.hitBody == b.hitBody && a.hurtBody == b.hurtBody 
                && a.hit.transform == b.hit.transform;
        }

        public static bool operator !=(HitData a, HitData b)
        {
            return !(a == b);
        }
    }
}