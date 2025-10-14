using Shears.Logging;
using System;
using System.Collections.Generic;

namespace Shears.HitDetection
{
    public readonly struct HitData2D : IHitData<HitData2D, HitResult2D>
    {
        private readonly IHitDeliverer2D deliverer;
        private readonly IHitReceiver2D receiver;
        private readonly IHitBody2D hitBody;
        private readonly IHurtBody2D hurtBody;
        private readonly HitResult2D result;
        private readonly Dictionary<Type, IHitSubdata> data;

        public readonly IHitDeliverer2D Deliverer => deliverer;
        public readonly IHitReceiver2D Receiver => receiver;
        public readonly IHitBody2D HitBody => hitBody;
        public readonly IHurtBody2D HurtBody => hurtBody;
        public readonly HitResult2D Result => result;

        #region Interface Variables
        IHitDeliverer<HitData2D> IHitData<HitData2D, HitResult2D>.Deliverer => deliverer;
        IHitReceiver<HitData2D> IHitData<HitData2D, HitResult2D>.Receiver => receiver;
        IHitBody<HitData2D> IHitData<HitData2D, HitResult2D>.HitBody => hitBody;
        IHurtBody<HitData2D> IHitData<HitData2D, HitResult2D>.HurtBody => hurtBody;
        HitResult2D IHitData<HitData2D, HitResult2D>.Result => result;

        IHitDeliverer IHitData.Deliverer => deliverer;
        IHitReceiver IHitData.Receiver => receiver;
        IHitBody IHitData.HitBody => hitBody;
        IHurtBody IHitData.HurtBody => hurtBody;
        IHitResult IHitData.Result => result;
        #endregion

        public HitData2D(IHitDeliverer2D deliverer, IHitReceiver2D receiver, 
            IHitBody2D hitBody, IHurtBody2D hurtBody, 
            HitResult2D result, params IHitSubdata[] data)
        {
            this.deliverer = deliverer;
            this.receiver = receiver;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
            this.data = new();

            if (data == null)
                return;

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

        public readonly bool TryGetData<T>(out T data)
        {
            if (this.data.TryGetValue(typeof(T), out IHitSubdata value) && value is T typedValue)
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

        #region Operator Overrides
        public readonly override bool Equals(object obj)
        {
            return obj is HitData2D data &&
                   EqualityComparer<IHitDeliverer2D>.Default.Equals(Deliverer, data.Deliverer) &&
                   EqualityComparer<IHitReceiver2D>.Default.Equals(Receiver, data.Receiver) &&
                   EqualityComparer<IHitBody2D>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<IHurtBody2D>.Default.Equals(HurtBody, data.HurtBody) &&
                   EqualityComparer<HitResult2D>.Default.Equals(Result, data.Result);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Deliverer, Receiver, HitBody, HurtBody, Result);
        }

        public readonly override string ToString()
        {
            return $"HitData3D(Deliverer: {deliverer}, Receiver: {receiver}, HitBody: {hitBody}, HurtBody: {hurtBody}, Result: {result})";
        }

        public static bool operator ==(HitData2D a, HitData2D b)
        {
            return a.Deliverer == b.Deliverer && a.Receiver == b.Receiver
                && a.HitBody == b.HitBody && a.HurtBody == b.HurtBody
                && a.Result.Transform == b.Result.Transform;
        }

        public static bool operator !=(HitData2D a, HitData2D b)
        {
            return !(a == b);
        }
        #endregion
    }
}
