using System;
using UnityEngine;

namespace Shears
{
    public class TypeSelectorItemAttribute : Attribute
    {
        private readonly string menuPath;

        public string MenuPath => menuPath;

        public TypeSelectorItemAttribute(string menuPath)
        {
            this.menuPath = menuPath;
        }
    }
}
