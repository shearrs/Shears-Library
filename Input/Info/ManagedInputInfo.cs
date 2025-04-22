using UnityEngine;

namespace Shears.Input
{
    public struct ManagedInputInfo
    {
        public IManagedInput Input { get; private set; }
        public ManagedInputDevice Device { get; private set; }
        public ManagedInputPhase Phase { get; private set; }

        public ManagedInputInfo(IManagedInput input, ManagedInputPhase phase, ManagedInputDevice device)
        {
            Input = input;
            Phase = phase;
            Device = device;
        }
    }
}
