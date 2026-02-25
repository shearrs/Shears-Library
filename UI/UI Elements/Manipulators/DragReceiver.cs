using System;
using UnityEngine;

namespace Shears.UI
{
    public class DragReceiver : UIManipulator
    {
        public event Action<DraggableElement> DragReceived;

        protected override void RegisterEvents()
        {
        }

        protected override void DeregisterEvents()
        {
        }

        internal void ReceiveDrag(DraggableElement element)
        {
            if (IsEnabled)
                DragReceived?.Invoke(element);
        }
    }
}
