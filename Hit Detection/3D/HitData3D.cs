using System.Collections.Generic;
using System.Linq;

namespace Shears.HitDetection
{
    public readonly struct HitData3D
    {
        private readonly HitBody3D hitBody;
        private readonly HurtBody3D hurtBody;
        private readonly HitResult3D result;
        private readonly IReadOnlyCollection<IHitSubdata> data;
        private readonly int dataCount;
        private readonly bool blocked;

        public readonly HitBody3D HitBody => hitBody;
        public readonly HurtBody3D HurtBody => hurtBody;
        public readonly HitResult3D Result => result;
        public readonly int DataCount => dataCount;
        public readonly bool Blocked => blocked;

        internal HitData3D(HitBody3D hitBody, HurtBody3D hurtBody,
            HitResult3D result, IReadOnlyCollection<IHitSubdata> data,
            bool blocked)
        {
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
            this.data = data;
            dataCount = (data == null) ? 0 : data.Count;
            this.blocked = blocked;
        }

        public readonly IHitSubdata GetDataAt(int index) => data.ElementAt(index);

        public readonly bool TryGetData<T>(out T data) where T : IHitSubdata
        {
            if (this.data == null)
            {
                data = default;
                return false;
            }

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