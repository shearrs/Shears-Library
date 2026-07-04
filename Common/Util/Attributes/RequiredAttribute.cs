using System;
using UnityEngine;

namespace Shears
{
    public class RequiredAttribute : PropertyAttribute
    {
        public string[] AlternativeValues { get; }

        public int TargetCollectionSize { get; }

        public RequiredAttribute(string alternativeValue = null, int targetCollectionSize = -1)
            : base(true)
        {
            if (alternativeValue == null)
                AlternativeValues = Array.Empty<string>();
            else
                AlternativeValues = new[] { alternativeValue };

            TargetCollectionSize = targetCollectionSize;
        }

        public RequiredAttribute(params string[] alternativeValues)
            : base(true)
        {
            AlternativeValues = alternativeValues;
            TargetCollectionSize = -1;
        }
    }
}
