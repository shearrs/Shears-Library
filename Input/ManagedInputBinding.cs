using UnityEngine;

namespace Shears.Input
{
    internal struct ManagedInputBinding
    {
        public ManagedInputPhase Phase { get; private set; }
        public ManagedInputEvent Action { get; private set; }

        public ManagedInputBinding(ManagedInputPhase phase, ManagedInputEvent action)
            => (Phase, Action) = (phase, action);
    }
}
