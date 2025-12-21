using UnityEngine;

namespace Shears.Signals
{
    public readonly struct ToggleInputSignal : ISignal
    {
        private readonly bool toggle;

        public readonly bool Toggle => toggle;

        public ToggleInputSignal(bool toggle)
        {
            this.toggle = toggle;
        }
    }
}
