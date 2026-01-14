using System;
using UnityEngine;

namespace Shears
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ReadOnlyAttribute : PropertyAttribute 
    { 
    }
}