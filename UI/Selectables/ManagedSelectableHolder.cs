using UnityEngine;

namespace Shears.UI
{
    public class ManagedSelectableHolder : MonoBehaviour
    {
        [SerializeField, ReadOnly] private ManagedSelectable heldSelectable;

        public void StoreCurrentSelectable()
        {
            heldSelectable = ManagedEventSystem.GetSelection();
        }

        public void AssignCurrentSelectable()
        {
            ManagedEventSystem.Select(heldSelectable);
        }
    }
}
