using UnityEngine;

namespace Shears
{
    public class RequiredAttribute : PropertyAttribute
    {
        public string AlternativeValue { get; }

        public int TargetCollectionSize { get; }

        public RequiredAttribute(string alternativeValue = null, int targetCollectionSize = -1)
            : base(true)
        {
            AlternativeValue = alternativeValue;
            TargetCollectionSize = targetCollectionSize;
        }
    }
}
