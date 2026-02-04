using System;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AutoPropertyAttribute : Attribute
    {
        public string Name { get; }

        public AutoPropertyAttribute(string name = "")
        {
            Name = name;
        }
    }
}
