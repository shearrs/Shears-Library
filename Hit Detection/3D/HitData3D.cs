using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitData3D : IHitData
    {
        public static HitData3D Empty = new(null, null, null, null, default);

        private readonly IHitDeliverer deliverer;
        private readonly IHitReceiver receiver;
        private readonly HitBody3D hitBody;
        private readonly HurtBody3D hurtBody;
        private readonly HitResult3D result;

        public readonly IHitDeliverer Deliverer => deliverer;
        public readonly IHitReceiver Receiver => receiver;
        public readonly HitBody3D HitBody => hitBody;
        public readonly HurtBody3D HurtBody => hurtBody;
        public readonly HitResult3D Result => result;

        readonly IHitBody IHitData.HitBody => hitBody;
        readonly IHurtBody IHitData.HurtBody => hurtBody;
        readonly IHitResult IHitData.Result => result;

        public HitData3D(IHitDeliverer deliverer, IHitReceiver receiver, HitBody3D hitBody, HurtBody3D hurtBody, HitResult3D result)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is HitData3D data &&
                   EqualityComparer<IHitDeliverer>.Default.Equals(Deliverer, data.Deliverer) &&
                   EqualityComparer<IHitReceiver>.Default.Equals(Receiver, data.Receiver) &&
                   EqualityComparer<HitBody3D>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<HurtBody3D>.Default.Equals(HurtBody, data.HurtBody) &&
                   EqualityComparer<HitResult3D>.Default.Equals(Result, data.Result);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Deliverer, Receiver, HitBody, HurtBody, Result);
        }

        public static bool operator==(HitData3D a, HitData3D b)
        {
            return a.Deliverer == b.Deliverer && a.Receiver == b.Receiver 
                && a.HitBody == b.HitBody && a.HurtBody == b.HurtBody 
                && a.Result.Transform == b.Result.Transform;
        }

        public static bool operator !=(HitData3D a, HitData3D b)
        {
            return !(a == b);
        }
    }
}