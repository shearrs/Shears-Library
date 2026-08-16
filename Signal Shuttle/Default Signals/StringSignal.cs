using UnityEngine;

namespace Shears.Signals
{
    public readonly struct StringSignal : ISignal
    {
        public string Value { get; }

        public StringSignal(string value)
        {
            Value = value;
        }
    }
}
