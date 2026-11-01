using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Shears
{
    [Serializable]
    public struct ShearsID
    {
        [SerializeField]
        private long mostSignificantBits;

        [SerializeField]
        private long leastSignificantBits;

        public static ShearsID Create()
        {
            var id = new ShearsID();
            var guid = Guid.NewGuid();

            var guidSpan = MemoryMarshal.CreateReadOnlySpan(ref guid, 1);
            var longSpan = MemoryMarshal.Cast<Guid, long>(guidSpan);

            id.mostSignificantBits = longSpan[0];
            id.leastSignificantBits = longSpan[1];

            return id;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ShearsID id && Equals(id);
        }

        public readonly bool Equals(ShearsID id)
        {
            return id.mostSignificantBits == mostSignificantBits
                && id.leastSignificantBits == leastSignificantBits;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(mostSignificantBits, leastSignificantBits);
        }
    }
}
