using UnityEngine;

namespace Shears.UI
{
    public class UIEvent
    {
        public bool WillTrickleDown { get; private set; } = true;

        public void PreventTrickleDown() => WillTrickleDown = false;
    }
}
