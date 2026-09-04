using UnityEngine;

namespace Shears.Grids
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public abstract class PathNodeData : GridNodeData
    {
        private readonly CompositeFalseFlag isBlocked = new();

        public bool IsBlocked => isBlocked.Value;

        public int AddBlockedReason()
        {
            int reasonID = isBlocked.AddReason();
            BroadcastUpdate();

            return reasonID;
        }

        public bool RemoveBlockedReason(int reasonID)
        {
            if (isBlocked.RemoveReason(reasonID))
            {
                BroadcastUpdate();
                return true;
            }

            return false;
        }
    }
}
