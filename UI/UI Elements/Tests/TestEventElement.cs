using UnityEngine;

namespace Shears.UI
{
    public class TestEventElement : UIElement
    {
        protected override void RegisterEvents()
        {
            RegisterEvent<HoverEnterEvent>(OnHoverEnter);
            RegisterEvent<HoverExitEvent>(OnHoverExit);
            RegisterEvent<PointerDownEvent>(OnPointerDown);
            RegisterEvent<PointerUpEvent>(OnPointerUp);
            RegisterEvent<ClickEvent>(OnClicked);
        }

        private void OnHoverEnter(HoverEnterEvent evt)
        {
            Log("Hover enter");
        }

        private void OnHoverExit(HoverExitEvent evt)
        {
            Log("Hover exit");
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            Log("Pointer down");
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            Log("Pointer up");
        }

        private void OnClicked(ClickEvent evt)
        {
            Log("Clicked");
        }
    }
}
