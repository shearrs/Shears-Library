using UnityEngine;

namespace Shears.UI
{
    public interface IUIManipulator
    {
        public bool IsEnabled { get; }

        public void Enable();

        public void Disable();
    }
}
