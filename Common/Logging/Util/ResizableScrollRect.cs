using UnityEngine;
using UnityEngine.UI;

namespace Shears.Logging
{
    /// <summary>
    /// Overrides the automatic resizing of <see cref="ScrollRect"/>'s vertical scrollbar.
    /// </summary>
    public class ResizableScrollRect : ScrollRect
    {
        [Header("Custom Fixes")]
        [SerializeField]
        private float _scrollBarSize = 0.1f;

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (verticalScrollbar)
                verticalScrollbar.size = _scrollBarSize;
        }

        public override void Rebuild(CanvasUpdate executing)
        {
            base.Rebuild(executing);

            if (verticalScrollbar)
                verticalScrollbar.size = _scrollBarSize;
        }
    }
}
