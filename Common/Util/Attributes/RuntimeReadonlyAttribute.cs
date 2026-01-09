using System;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class RuntimeReadOnlyAttribute : Attribute
    {
    }
}
