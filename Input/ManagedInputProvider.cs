using UnityEngine;

namespace Shears.Input
{
    public abstract class ManagedInputProvider : ScriptableObject
    {
        public abstract IManagedInput GetInput(string name);
    }
}
