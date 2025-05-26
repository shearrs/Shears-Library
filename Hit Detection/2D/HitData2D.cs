using System;
using System.Collections.Generic;

namespace Shears.HitDetection
{
    public struct HitData2D : IHitData
    {
        public static HitData2D Empty = new(null, null, null, null, default);

        private readonly IHitDeliverer<HitData2D> deliverer;
        private readonly IHitReceiver<HitData2D> receiver;
        private readonly IHitBody<HitData2D> hitBody;
        private readonly IHurtBody<HitData2D> hurtBody;
        private readonly HitResult2D result;

        public readonly IHitDeliverer<HitData2D> Deliverer => deliverer;
        public readonly IHitReceiver<HitData2D> Receiver => receiver;
        public readonly IHitBody<HitData2D> HitBody => hitBody;
        public readonly IHurtBody<HitData2D> HurtBody => hurtBody;
        public readonly HitResult2D Result => result;

        readonly IHitBody IHitData.HitBody => hitBody;
        readonly IHurtBody IHitData.HurtBody => hurtBody;
        readonly IHitResult IHitData.Result => result;
        readonly IHitDeliverer IHitData.Deliverer => Deliverer;
        readonly IHitReceiver IHitData.Receiver => Receiver;

        public HitData2D(IHitDeliverer<HitData2D> deliverer, IHitReceiver<HitData2D> receiver, IHitBody<HitData2D> hitBody, IHurtBody<HitData2D> hurtBody, HitResult2D result)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is HitData2D data &&
                   EqualityComparer<IHitDeliverer>.Default.Equals(Deliverer, data.Deliverer) &&
                   EqualityComparer<IHitReceiver>.Default.Equals(Receiver, data.Receiver) &&
                   EqualityComparer<IHitBody<HitData2D>>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<IHurtBody<HitData2D>>.Default.Equals(HurtBody, data.HurtBody) &&
                   EqualityComparer<HitResult2D>.Default.Equals(result, data.result);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(deliverer, receiver, hitBody, hurtBody, result);
        }

        public static bool operator ==(HitData2D a, HitData2D b)
        {
            return a.deliverer == b.deliverer && a.receiver == b.receiver
                && a.hitBody == b.hitBody && a.hurtBody == b.hurtBody
                && a.result.Transform == b.result.Transform;
        }

        public static bool operator !=(HitData2D a, HitData2D b)
        {
            return !(a == b);
        }
    }
}
