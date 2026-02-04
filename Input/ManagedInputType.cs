using UnityEngine;

namespace Shears.Input
{
    public abstract class ManagedInputType
    {
        public static ManagedInputType GetMostRecentInputType()
        {
            if (ManagedGamepad.Current.WasUpdatedThisFrame)
                return ManagedGamepad.Current;
            else
                return ManagedPointer.Current;
        }
    }
}
