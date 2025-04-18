using Shears.Common;
using Shears.Input;
using System;
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
        private bool isEnabled = false;

        public static event Action<ManagedSelectable> OnSelectionChanged
        {
            add => Instance.InstOnSelectionChanged += value;
            remove
            {
                if (instance == null)
                    return;

                Instance.InstOnSelectionChanged -= value;
            }
        }
        private event Action<ManagedSelectable> InstOnSelectionChanged;

        private void OnEnable()
        {
            eventSystem = EventSystem.current;

            submitInput ??= inputProvider.GetInput("Submit");

            submitInput.Performed += SubmitSelection;

            InstEnable();
        }

        private void OnDisable()
        {
            submitInput.Performed -= SubmitSelection;

            InstDisable();
        }

        private void Update()
        {
            UpdateManagedSelection();
        }

        public static void Enable() => Instance.InstEnable();
        private void InstEnable()
        {
            if (isEnabled)
                return;

            if (eventSystem.currentInputModule != null)
                eventSystem.currentInputModule.enabled = true;

            isEnabled = true;
        }

        public static void Disable() => Instance.InstDisable();
        private void InstDisable()
        {
            if (!isEnabled)
                return;

            if (eventSystem.currentInputModule != null)
                eventSystem.currentInputModule.enabled = false;

            isEnabled = false;
        }

        #region Selection
        private void UpdateManagedSelection()
        {
            var currentObject = eventSystem.currentSelectedGameObject;

            if (currentObject == null || !currentObject.TryGetComponent(out ManagedSelectable selectable) || selection == selectable)
                return;

            ClearSelection();

            if (selectable.Interactable)
            {
                selection = selectable;
                selection.Select();

                InstOnSelectionChanged?.Invoke(selection);
            }
        }

        public static ManagedSelectable GetSelection() => Instance.InstGetSelection();
        private ManagedSelectable InstGetSelection() => selection;

        public static void Select(ManagedSelectable selectable) => Instance.InstSelect(selectable);
        private void InstSelect(ManagedSelectable selectable)
        {
            if (selectable == null)
            {
                eventSystem.SetSelectedGameObject(null);
                UpdateManagedSelection();

                return;
            }
            
            if (!selectable.Interactable)
                return;

            eventSystem.SetSelectedGameObject(selectable.gameObject);
            UpdateManagedSelection();
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
