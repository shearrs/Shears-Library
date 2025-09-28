using System;
using UnityEngine;

namespace Shears
{
    public class FoldoutGroupAttribute : Attribute
    {
        private readonly string name;
        private readonly int fieldCount;
        private readonly bool expanded;

        public string Name => name;
        public int FieldCount => fieldCount;
        public bool Expanded => expanded;

        public FoldoutGroupAttribute(string name, int fieldCount = 0, bool expanded = false)
        {
            this.name = name;
            this.fieldCount = fieldCount;
            this.expanded = expanded;
        }
    }
}
