using UnityEngine;

namespace Shears.Signals
{
    public delegate void SignalListener<T>(in T signal)
        where T : ISignal;
}
