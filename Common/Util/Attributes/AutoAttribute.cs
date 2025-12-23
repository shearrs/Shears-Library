using JetBrains.Annotations;
using System;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    [MeansImplicitUse]
    public class AutoAttribute : Attribute
    {
    }
}
