using UnityEngine;

namespace Shears
{
    public class RequiredAttribute : PropertyAttribute
    {
        public string AlternativeValue { get; private set; }

        public RequiredAttribute(string alternativeValue = null)
        {
            AlternativeValue = alternativeValue;
        }
    }
}
