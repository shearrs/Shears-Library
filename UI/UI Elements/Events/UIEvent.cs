using UnityEngine;

namespace Shears.UI
{
    public class UIEvent
    {
        public bool IsTricklingDown { get; internal set; } = false;
        public bool IsBubblingUp { get; internal set; } = false;
        public bool TrickleDown { get; protected set; } = true;
        public bool BubbleUp { get; protected set; } = true;

        public void PreventTrickleDown() => TrickleDown = false;

        public void PreventBubbleUp() => BubbleUp = false;

        public void PreventDefault()
        {
            TrickleDown = false;
            BubbleUp = false;
        }
    }
}
