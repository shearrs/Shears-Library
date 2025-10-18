using Shears.Logging;
using System;
using System.Collections.Generic;

namespace Shears.HitDetection
{
    public readonly struct HitData3D : IHitData<HitData3D, HitResult3D>
    {
        private readonly IHitDeliverer3D deliverer;
        private readonly IHitReceiver3D receiver;
        private readonly IHitBody3D hitBody;
        private readonly IHurtBody3D hurtBody;
        private readonly HitResult3D result;
        private readonly IReadOnlyCollection<IHitSubdata> data; // IMPORTANT !!! this is NOT being copied by default, if we are changing damage values often, we need to copy our collections

        public readonly IHitDeliverer3D Deliverer => deliverer;
        public readonly IHitReceiver3D Receiver => receiver;
        public readonly IHitBody3D HitBody => hitBody;
        public readonly IHurtBody3D HurtBody => hurtBody;
        public readonly HitResult3D Result => result;
        public readonly IReadOnlyCollection<IHitSubdata> Data => data;

        #region Interface Variables
        IHitDeliverer<HitData3D> IHitData<HitData3D, HitResult3D>.Deliverer => deliverer;
        IHitReceiver<HitData3D> IHitData<HitData3D, HitResult3D>.Receiver => receiver;
        IHitBody<HitData3D> IHitData<HitData3D, HitResult3D>.HitBody => hitBody;
        IHurtBody<HitData3D> IHitData<HitData3D, HitResult3D>.HurtBody => hurtBody;
        HitResult3D IHitData<HitData3D, HitResult3D>.Result => result;

        IHitDeliverer IHitData.Deliverer => deliverer;
        IHitReceiver IHitData.Receiver => receiver;
        IHitBody IHitData.HitBody => hitBody;
        IHurtBody IHitData.HurtBody => hurtBody;
        IHitResult IHitData.Result => result;
        #endregion

        public HitData3D(IHitDeliverer3D deliverer, IHitReceiver3D receiver,
            IHitBody3D hitBody, IHurtBody3D hurtBody,
            HitResult3D result, IReadOnlyCollection<IHitSubdata> data)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
            this.data = data;
        }

        public readonly bool TryGetData<T>(out T data) where T : IHitSubdata
        {
            foreach (var hitData in this.data)
            {
                if (hitData is T typedData)
                {
                    data = typedData;
                    return true;
                }
            }

            data = default;
            return false;
        }
    }
}
