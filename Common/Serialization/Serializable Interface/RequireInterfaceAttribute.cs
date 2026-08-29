using System;
using UnityEngine;

namespace Shears
{
    /// <summary>
    /// Attribute that enforces the implementation of a specific interface on a serialized field.
    /// </summary>
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
