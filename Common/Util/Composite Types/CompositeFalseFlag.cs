using System.Collections.Generic;
using UnityEngine;

namespace Shears
{
    public class CompositeFalseFlag
    {
        private readonly List<int> reasons = new();
        private int reasonID = 0;

        public bool Value => reasons.Count == 0;
        public int ReasonCount => reasons.Count;

        public int AddReason()
        {
            reasons.Add(reasonID);

            return reasonID++;
        }

        public void RemoveReason(int reason)
        {
            reasons.Remove(reason);
        }

        public static implicit operator bool(CompositeFalseFlag other) => other.Value;
    }
}
