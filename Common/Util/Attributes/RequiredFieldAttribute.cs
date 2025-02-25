using UnityEngine;

namespace Shears
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class RequiredFieldAttribute : PropertyAttribute 
    { 
        public Color Color { get; set; }

        public RequiredFieldAttribute()
        {
            Color = Color.red;
        }

        public RequiredFieldAttribute(Color color)
        {
            Color = color;
        }
    }
}
