using System;
using UnityEngine;

namespace Shears
{
    public class CustomWrapperAttribute : Attribute
    {
        public string[] DisplayFields { get; set; } = null;
        public bool ShowAllFields { get; set; } = false;

        public CustomWrapperAttribute()
        {

        }
    }
}
