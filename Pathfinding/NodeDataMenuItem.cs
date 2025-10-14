using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeDataMenuItem : Attribute
    {
        private readonly string menuPath;

        public string MenuPath => menuPath;

        public NodeDataMenuItem(string menuPath)
        {
            this.menuPath = menuPath;
        }
    }
}
