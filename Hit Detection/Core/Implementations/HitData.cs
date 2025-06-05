using Shears.Logging;
using System;
using System.Collections.Generic;

namespace Shears.HitDetection
{
    public readonly struct HitData<HitResultType> : IHitData where HitResultType : IHitResult
    {
        public static readonly HitData<HitResultType> Empty = new(null, null, null, null, default);

        private readonly IHitDeliverer<HitData<HitResultType>> deliverer;
        private readonly IHitReceiver<HitData<HitResultType>> receiver;
        private readonly IHitBody<HitData<HitResultType>> hitBody;
        private readonly IHurtBody<HitData<HitResultType>> hurtBody;
        private readonly HitResultType result;
        private readonly Dictionary<Type, object> data;

        public readonly IHitDeliverer<HitData<HitResultType>> Deliverer => deliverer;
        public readonly IHitReceiver<HitData<HitResultType>> Receiver => receiver;
        public readonly IHitBody<HitData<HitResultType>> HitBody => hitBody;
        public readonly IHurtBody<HitData<HitResultType>> HurtBody => hurtBody;
        public readonly HitResultType Result => result;

        readonly IHitBody IHitData.HitBody => hitBody;
        readonly IHurtBody IHitData.HurtBody => hurtBody;
        readonly IHitResult IHitData.Result => result;
        readonly IHitDeliverer IHitData.Deliverer => Deliverer;
        readonly IHitReceiver IHitData.Receiver => Receiver;

        public HitData(IHitDeliverer<HitData<HitResultType>> deliverer, IHitReceiver<HitData<HitResultType>> receiver, 
                        IHitBody<HitData<HitResultType>> hitBody, IHurtBody<HitData<HitResultType>> hurtBody, 
                        HitResultType result, params object[] data)
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
                        SHLogger.Log($"Data of type {type} already exists in HitData3D.", SHLogLevel.Warning);
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

        #region Operator Overrides
        public readonly override bool Equals(object obj)
        {
            return obj is HitData<HitResultType> data &&
                   EqualityComparer<IHitDeliverer<HitData<HitResultType>>>.Default.Equals(Deliverer, data.Deliverer) &&
                   EqualityComparer<IHitReceiver<HitData<HitResultType>>>.Default.Equals(Receiver, data.Receiver) &&
                   EqualityComparer<IHitBody<HitData<HitResultType>>>.Default.Equals(HitBody, data.HitBody) &&
                   EqualityComparer<IHurtBody<HitData<HitResultType>>>.Default.Equals(HurtBody, data.HurtBody) &&
                   EqualityComparer<HitResultType>.Default.Equals(Result, data.Result);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Deliverer, Receiver, HitBody, HurtBody, Result);
        }

        public readonly override string ToString()
        {
            return $"HitData3D(Deliverer: {deliverer}, Receiver: {receiver}, HitBody: {hitBody}, HurtBody: {hurtBody}, Result: {result})";
        }

        public static bool operator ==(HitData<HitResultType> a, HitData<HitResultType> b)
        {
            return a.Deliverer == b.Deliverer && a.Receiver == b.Receiver
                && a.HitBody == b.HitBody && a.HurtBody == b.HurtBody
                && a.Result.Transform == b.Result.Transform;
        }

        public static bool operator !=(HitData<HitResultType> a, HitData<HitResultType> b)
        {
            return !(a == b);
        }
        #endregion
    }
}
