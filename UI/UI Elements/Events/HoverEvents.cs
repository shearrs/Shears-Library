using UnityEngine;

namespace Shears.UI
{
    public class HoverEnterEvent : UIEvent
    {
        public HoverEnterEvent()
        {
            TrickleDown = false;
        }
    }

    public class HoverExitEvent : UIEvent
    {
        public HoverExitEvent()
        {
            TrickleDown = false;
        }
    }
}
