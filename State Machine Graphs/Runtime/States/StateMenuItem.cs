using System;
using UnityEngine;

namespace Shears.StateMachineGraphs
{
    public class StateMenuItem : Attribute
    {
        private readonly string menuPath;

        public string MenuPath => menuPath;

        public StateMenuItem(string menuPath)
        {
            this.menuPath = menuPath;
        }
    }
}
