using System;
using UnityEngine;

namespace Shears.Pathfinding
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeDataMenuItem : Attribute
    {
        private readonly string menuPath;
        private readonly int order;

        public string MenuPath => menuPath;
        public int Order => order;

        public NodeDataMenuItem(string menuPath, int order = -1)
        {
            this.menuPath = menuPath;
            this.order = order;
        }
    }
}
