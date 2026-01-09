using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public partial class CellContent : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer;

        [Auto]
        private UIElement element;

        private ColorModulator colorModulator;
        private Vector3 offset;

        private void Start()
        {
            colorModulator = new(element, () => true, renderer.material);

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
