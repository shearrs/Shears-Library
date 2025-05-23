using System;
using System.Collections.Generic;

namespace Shears.HitDetection
{
    public struct HitData2D : IHitData
    {
        public static HitData2D Empty = new(null, null, null, null, default);

        private readonly IHitDeliverer deliverer;
        private readonly IHitReceiver receiver;
        private readonly HitBody2D hitBody;
        private readonly HurtBody2D hurtBody;
        private readonly HitResult2D result;

        public readonly IHitDeliverer Deliverer => deliverer;
        public readonly IHitReceiver Receiver => receiver;
        public readonly HitBody2D HitBody => hitBody;
        public readonly HurtBody2D HurtBody => hurtBody;

        readonly IHitBody IHitData.HitBody => hitBody;
        readonly IHurtBody IHitData.HurtBody => hurtBody;

        readonly IHitResult IHitData.Result => result;

        public HitData2D(IHitDeliverer deliverer, IHitReceiver receiver, HitBody2D hitBody, HurtBody2D hurtBody, HitResult2D result)
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
                   EqualityComparer<HitBody2D>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<HurtBody2D>.Default.Equals(HurtBody, data.HurtBody) &&
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
