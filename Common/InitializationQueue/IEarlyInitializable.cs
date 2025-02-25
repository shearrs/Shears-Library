using UnityEngine;

namespace Shears.EarlyInitialization
{
    public enum InitializationOrder { First = 0, Second = 1, Third = 2 }

    public interface IEarlyInitializable
    {
        public InitializationOrder Order { get; }

        public void EarlyInitialize();
    }
}
