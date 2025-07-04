using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.HitDetection
{
    public readonly struct HitData3D : IHitData<HitData3D, HitResult3D>
    {
        private readonly IHitDeliverer3D deliverer;
        private readonly IHitReceiver3D receiver;
        private readonly IHitBody3D hitBody;
        private readonly IHurtBody3D hurtBody;
        private readonly HitResult3D result;
        private readonly Dictionary<Type, object> data;

        public IHitDeliverer3D Deliverer => deliverer;
        public IHitReceiver3D Receiver => receiver;
        public IHitBody3D HitBody => hitBody;
        public IHurtBody3D HurtBody => hurtBody;
        public HitResult3D Result => result;

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
    HitResult3D result, params object[] data)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
            this.data = new();

            foreach (var item in data)
            {
                if (item != null)
                {
                    Type type = item.GetType();

                    if (!this.data.ContainsKey(type))
                        this.data[type] = item;
                    else
                        SHLogger.Log($"Data of type {type} already exists in HitData3D.", SHLogLevels.Warning);
                }
            }
        }

        public bool TryGetData<T>(out T data)
        {
            if (this.data.TryGetValue(typeof(T), out object value) && value is T typedValue)
            {
                data = typedValue;

                return true;
            }
            else
            {
                data = default;

                return false;
            }
        }
    }
}
