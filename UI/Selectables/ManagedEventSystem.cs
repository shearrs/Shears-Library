using Shears.Common;
using Shears.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shears.UI
{
    [DefaultExecutionOrder(-100)]
    public class ManagedEventSystem : ProtectedSingleton<ManagedEventSystem>
    {
        [SerializeField] private ManagedInputProvider inputProvider;
        [SerializeField] private ManagedSelectable selection;

        private EventSystem eventSystem;

        private IManagedInput submitInput;

        private void OnEnable()
        {
            eventSystem = EventSystem.current;

            submitInput ??= inputProvider.GetInput("Submit");

            submitInput.Performed += SubmitSelection;
        }

        private void OnDisable()
        {
            submitInput.Performed -= SubmitSelection;
        }

        private void Update()
        {
            UpdateSelection();
        }

        #region Selection
        private void UpdateSelection()
        {
            var currentObject = eventSystem.currentSelectedGameObject;

            if (currentObject != null && currentObject.TryGetComponent(out ManagedSelectable selectable))
            {
                if (!selectable.Interactable)
                    ClearSelection();
                else if (selection == selectable)
                    return;
                else
                {
                    ClearSelection();

                    selection = selectable;
                    selection.Select();
                }
            }
            else
            {
                ClearSelection();
            }
        }

        public static ManagedSelectable GetSelection() => Instance.InstGetSelection();
        private ManagedSelectable InstGetSelection() => selection;

        public static void Select(ManagedSelectable selectable) => Instance.InstSelect(selectable);
        private void InstSelect(ManagedSelectable selectable)
        {
            if (!selectable.Interactable)
                return;

            eventSystem.SetSelectedGameObject(selectable.gameObject);
            UpdateSelection();
        }

        private void ClearSelection()
        {
            if (selection != null)
                selection.Unselect();

            selection = null;
        }

        private void SubmitSelection(ManagedInputInfo info)
        {
            var selectable = GetSelection();

            if (!selectable.TryGetComponent(out ManagedButton button))
                return;

            button.Click();
        }
        #endregion
    }
}
