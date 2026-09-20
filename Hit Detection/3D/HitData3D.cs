using System.Collections.Generic;
using System.Linq;

namespace Shears.HitDetection
{
    public class HitData3D
    {
        private readonly HitShape3D hitShape;
        private readonly HitBody3D hitBody;
        private readonly HurtBody3D hurtBody;
        private readonly HitResult3D result;
        private readonly IReadOnlyCollection<IHitSubdata> data;
        private readonly int dataCount;
        private readonly bool blocked;

        public HitShape3D HitShape => hitShape;
        public HitBody3D HitBody => hitBody;
        public HurtBody3D HurtBody => hurtBody;
        public HitResult3D Result => result;
        public int DataCount => dataCount;
        public bool Blocked => blocked;

        internal HitData3D(
            HitShape3D hitShape,
            HitBody3D hitBody,
            HurtBody3D hurtBody,
            HitResult3D result,
            IReadOnlyCollection<IHitSubdata> data,
            bool blocked
        )
        {
            this.hitShape = hitShape;
            this.hitBody = hitBody;
            this.hurtBody = hurtBody;
            this.result = result;
            this.data = data;
            dataCount = (data == null) ? 0 : data.Count;
            this.blocked = blocked;
        }

        public HitData3D(HurtBody3D hurtBody, params IHitSubdata[] data)
        {
            hitShape = null;
            hitBody = null;
            this.hurtBody = hurtBody;
            result = default;
            this.data = data;
            dataCount = (data == null) ? 0 : data.Length;
            blocked = false;
        }

        public IHitSubdata GetDataAt(int index) => data.ElementAt(index);

        public bool TryGetData<T>(out T data)
            where T : IHitSubdata
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
