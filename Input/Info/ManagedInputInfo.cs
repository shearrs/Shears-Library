using UnityEngine;

namespace Shears.Input
{
    public readonly struct ManagedInputInfo
    {
        private readonly IManagedInput input;
        private readonly ManagedInputDevice device;
        private readonly ManagedInputPhase phase;

        public readonly IManagedInput Input => input;
        public readonly ManagedInputDevice Device => device;
        public readonly ManagedInputPhase Phase => phase;

        public ManagedInputInfo(IManagedInput input, ManagedInputPhase phase, ManagedInputDevice device)
        {
            this.input = input;
            this.device = device;
            this.phase = phase;
        }
    }
}
