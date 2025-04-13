using UnityEngine;

namespace Shears.Input
{
    public readonly struct ManagedInputBindingInstance
    {
        private readonly IManagedInput input;
        private readonly ManagedInputPhase phase;
        private readonly ManagedInputEvent action;

        public ManagedInputBindingInstance(IManagedInput input, ManagedInputPhase phase, ManagedInputEvent action)
        {
            this.input = input;
            this.phase = phase;
            this.action = action;
        }

        public readonly void Bind()
        {
            input.Bind(phase, action);
        }

        public readonly void Unbind()
        {
            input.Unbind(phase, action);
        }
    }
}
