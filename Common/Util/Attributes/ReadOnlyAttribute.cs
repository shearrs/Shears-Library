using UnityEngine;

namespace Shears
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}