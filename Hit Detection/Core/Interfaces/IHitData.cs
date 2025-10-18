using UnityEngine;

namespace Shears.HitDetection
{
    public interface IHitData
    {
        public IHitDeliverer Deliverer { get; }
        public IHitReceiver Receiver { get; }
        public IHitBody HitBody { get; }
        public IHurtBody HurtBody { get; }
        public IHitResult Result { get; }

        public bool TryGetData<T>(out T data) where T : IHitSubdata;
    }

    public interface IHitData<HitDataType, HitResultType> : IHitData 
        where HitDataType : IHitData<HitDataType, HitResultType> 
        where HitResultType : IHitResult
    {
        new public IHitDeliverer<HitDataType> Deliverer { get; }
        new public IHitReceiver<HitDataType> Receiver { get; }
        new public IHitBody<HitDataType> HitBody { get; }
        new public IHurtBody<HitDataType> HurtBody { get; }
        new public HitResultType Result { get; }
    }
}
