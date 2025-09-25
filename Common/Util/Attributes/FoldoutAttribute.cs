using System;
using UnityEngine;

namespace Shears
{
    public class FoldoutGroupAttribute : Attribute
    {
        private readonly string name;

        public string Name => name;

        public FoldoutGroupAttribute(string name)
        {
            this.name = name;
        }

        // [FoldoutGroup("Settings")]
    }
}
