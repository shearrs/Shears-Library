using UnityEngine;

namespace Shears
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public bool Show { get; }

        public FoldoutAttribute(bool show = true)
        {
            Show = show;
        }
    }
}
