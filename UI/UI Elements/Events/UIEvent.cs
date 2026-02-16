using UnityEngine;

namespace Shears.UI
{
    public class UIEvent
    {
        public bool TrickleDown { get; protected set; } = true;
        public bool BubbleUp { get; protected set; } = false;

        public void PreventTrickleDown() => TrickleDown = false;
    }
}
