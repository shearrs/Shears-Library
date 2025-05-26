using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitData3D : IHitData
    {
        public static HitData3D Empty = new(null, null, null, null, default);

        private readonly IHitDeliverer<HitData3D> deliverer;
        private readonly IHitReceiver<HitData3D> receiver;
        private readonly IHitBody<HitData3D> hitBody;
        private readonly IHurtBody<HitData3D> hurtBody;
        private readonly HitResult3D result;

        public readonly IHitDeliverer<HitData3D> Deliverer => deliverer;
        public readonly IHitReceiver<HitData3D> Receiver => receiver;
        public readonly IHitBody<HitData3D> HitBody => hitBody;
        public readonly IHurtBody<HitData3D> HurtBody => hurtBody;
        public readonly HitResult3D Result => result;

        readonly IHitBody IHitData.HitBody => hitBody;
        readonly IHurtBody IHitData.HurtBody => hurtBody;
        readonly IHitResult IHitData.Result => result;

        readonly IHitDeliverer IHitData.Deliverer => Deliverer;
        readonly IHitReceiver IHitData.Receiver => Receiver;

        public HitData3D(IHitDeliverer<HitData3D> deliverer, IHitReceiver<HitData3D> receiver, IHitBody<HitData3D> hitBody, IHurtBody<HitData3D> hurtBody, HitResult3D result)
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
                   EqualityComparer<IHitDeliverer<HitData3D>>.Default.Equals(Deliverer, data.Deliverer) &&
                   EqualityComparer<IHitReceiver<HitData3D>>.Default.Equals(Receiver, data.Receiver) &&
                   EqualityComparer<IHitBody<HitData3D>>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<IHurtBody<HitData3D>>.Default.Equals(HurtBody, data.HurtBody) &&
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