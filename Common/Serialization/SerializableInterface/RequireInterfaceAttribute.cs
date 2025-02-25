using System;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type type)
        {
            Debug.Assert(type.IsInterface, $"{nameof(type)} needs to be an interface.");
            InterfaceType = type;
        }
    }
}
