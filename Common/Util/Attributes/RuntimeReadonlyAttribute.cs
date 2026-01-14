using System;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RuntimeReadOnlyAttribute : PropertyAttribute
    {
    }
}
