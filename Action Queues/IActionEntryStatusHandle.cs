using UnityEngine;

namespace Shears.ActionQueues
{
    public interface IActionEntryStatusHandle
    {
        public bool Success { get; }
    }
}
