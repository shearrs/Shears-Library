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
        private readonly RaycastHit2D[] raycastHits = new RaycastHit2D[10];

        protected override void Awake()
        {
            base.Awake();

            if (inputMap == null)
                inputMap = Resources.Load<ManagedInputMap>("ManagedElements/Shears_DefaultEventSystemInputMap");
        }

        private void OnEnable()
        {
            clickInput ??= inputMap.GetInput("Click");

            clickInput.Started += BeginClick;
            clickInput.Canceled += EndClick;

            inputMap.EnableAllInputs();
        }

        private void OnDisable()
        {
            clickInput.Started -= BeginClick;
            clickInput.Canceled -= EndClick;

            inputMap.DisableAllInputs();
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

            if (clickedElement != null && newHoverTarget != clickedElement)
            {
                hoveredElement = null;
                return;
            }

            hoveredElement = newHoverTarget;

            if (hoveredElement != null)
                hoveredElement.BeginHover();
        }

        // make a flag for elements to block raycast/not block
        // if not block, we continue looking
        // we still send events to all of the results up until a blocking one though
        private ManagedSpriteElement Raycast()
        {
            Vector2 pointerPos = ManagedPointer.Current.Position;
            var camera = Camera.main;
            var origin = camera.ScreenToWorldPoint(pointerPos);
            var filter = new ContactFilter2D
            {
                useTriggers = true
            };

            int hits = Physics2D.Raycast(origin, Vector2.zero, filter, raycastHits, 100f);

            for (int i = 0; i < hits; i++)
            {
                var hit = raycastHits[i];

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
