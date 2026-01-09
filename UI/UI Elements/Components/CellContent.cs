using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public partial class CellContent : MonoBehaviour
    {
        [Auto]
        private UIElement element;

        private Vector3 offset;

        private void Start()
        {
            element.RegisterEvent<DragBeginEvent>(OnDragBeginEvent);
            element.RegisterEvent<DragEvent>(OnDragEvent);
        }

        private void OnDragBeginEvent(DragBeginEvent evt)
        {
            offset = evt.PointerWorldOffset;
        }

        private void OnDragEvent(DragEvent evt)
        {
            element.transform.position = evt.PointerWorldPosition + offset;
        }
    }
}
