using Shears.Common;
using Shears.Input;
using UnityEngine;

namespace Shears.UI
{
    public class ManagedSpriteEventSystem : ProtectedSingleton<ManagedSpriteEventSystem>
    {
        [SerializeField] private ManagedInputMap inputMap;

        private ManagedSpriteElement clickedElement;
        private ManagedSpriteElement hoveredElement;

        private IManagedInput clickInput;
        private readonly RaycastHit[] raycastHits = new RaycastHit[100];

        private void OnEnable()
        {
            clickInput ??= inputMap.GetInput("Click");

            clickInput.Started += BeginClick;
            clickInput.Canceled += EndClick;
        }

        private void OnDisable()
        {
            clickInput.Started -= BeginClick;
            clickInput.Canceled -= EndClick;
        }

        private void Update()
        {
            UpdateHoveredElement();
        }

        private void UpdateHoveredElement()
        {
            ManagedSpriteElement newHoverTarget = Raycast();

            if (newHoverTarget == hoveredElement)
                return;

            if (hoveredElement != null)
                hoveredElement.EndHover();

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.BeginHover();
        }

        private ManagedSpriteElement Raycast()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(pointerPos);

            Physics.RaycastNonAlloc(ray, raycastHits, 100f, 0, QueryTriggerInteraction.Collide);

            foreach (var hit in raycastHits)
            {
                if (hit.collider == null)
                    continue;

                if (hit.collider.TryGetComponent<ManagedSpriteElement>(out var element))
                    return element;
            }

            return null;
        }

        private void BeginClick(ManagedInputInfo info)
        {
            if (hoveredElement == null)
                return;

            clickedElement = hoveredElement;

            clickedElement.BeginClick();
        }

        private void EndClick(ManagedInputInfo info)
        {
            if (clickedElement != null)
                clickedElement.EndClick();

            clickedElement = null;
        }
    }
}
