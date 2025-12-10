using UnityEngine;

namespace Shears.Input
{
    internal interface IManagedInputBinding
    {
        public ManagedInputPhase Phase { get; }
    }

    internal readonly struct ManagedInputBinding : IManagedInputBinding
    {
        private readonly ManagedInputPhase phase;
        private readonly ManagedInputEvent action;

        public readonly ManagedInputPhase Phase => phase;
        public readonly ManagedInputEvent Action => action;

        public ManagedInputBinding(ManagedInputPhase phase, ManagedInputEvent action)
            => (this.phase, this.action) = (phase, action);
    }

    internal readonly struct ManagedInputBindingWithInfo : IManagedInputBinding
    {
        private readonly ManagedInputPhase phase;
        private readonly ManagedInputEventWithInfo action;

        public readonly ManagedInputPhase Phase => phase;
        public readonly ManagedInputEventWithInfo Action => action;

        public ManagedInputBindingWithInfo(ManagedInputPhase phase, ManagedInputEventWithInfo action)
            => (this.phase, this.action) = (phase, action);
    }
}
