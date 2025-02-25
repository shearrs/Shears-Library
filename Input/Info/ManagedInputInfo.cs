using UnityEngine;

namespace Shears.Input
{
    public struct ManagedInputInfo
    {
        public IManagedInput Input { get; private set; }
        public ManagedInputDevice Device { get; private set; }

        public ManagedInputInfo(IManagedInput input, ManagedInputDevice device)
        {
            Input = input;
            Device = device;
        }
    }
}
