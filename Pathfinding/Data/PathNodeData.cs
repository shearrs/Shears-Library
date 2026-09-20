using Shears.Grids;
using UnityEngine;

namespace Shears.Pathfinding
{
    [System.Serializable]
    [DisallowMultipleComponent]
    public abstract class PathNodeData : GridNodeData
    {
        private readonly CompositeFalseFlag isBlocked = new(false);

        public virtual bool IsBlocked => isBlocked.Value;

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
