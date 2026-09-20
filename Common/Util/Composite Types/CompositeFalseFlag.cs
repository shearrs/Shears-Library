using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class CompositeFalseFlag
    {
        private readonly List<int> reasons = new();
        private int reasonID = 0;

        public bool ZeroIsTrue { get; set; } = true;
        public bool Value => (reasons.Count == 0) == ZeroIsTrue;
        public int ReasonCount => reasons.Count;

        public CompositeFalseFlag(bool zeroIsTrue = true)
        {
            ZeroIsTrue = zeroIsTrue;
        }

        public int AddReason()
        {
            reasons.Add(reasonID);

            return reasonID++;
        }

        public bool RemoveReason(int reason)
        {
            return reasons.Remove(reason);
        }

        public static implicit operator bool(CompositeFalseFlag other) => other.Value;
    }
}
