using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    [SuppressMessage("Style", "IDE0044:Make field readonly", Justification = "Assigned automatically in Awake()")]
    public class AutoAttribute : Attribute
    {
    }
}
