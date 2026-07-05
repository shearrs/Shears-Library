using UnityEngine;

namespace Shears.UI
{
    public class PointerDownEvent : UIEvent
    {
        public PointerDownEvent()
        {
            TrickleDown = false;
        }
    }

    public class PointerUpEvent : UIEvent
    {
        public PointerUpEvent()
        {
            TrickleDown = false;
        }
    }
}
