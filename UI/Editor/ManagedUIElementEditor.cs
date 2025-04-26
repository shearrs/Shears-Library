using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedUIElement))]
    public class ManagedUIElementEditor : UnityEditor.Editor
    {
        #region Tooltips
        private static readonly string FlagsFoldoutTooltip = "Status flags for the UI Element.";
        private static readonly string ActivationFoldoutTooltip = "Events for activation.\nOnEnabled - When 'Enable()' is called on this UIElement.\nOnDisabled - When 'Disable()' is called on this UIElement.";
        private static readonly string SelectFoldoutTooltip = "Events for selection.\nOnSelectBegin - When user select input goes down while this UIElement is focused.\nOnSelectEnd - When user select input goes up while this UIElement is focused.";
        private static readonly string ClickFoldoutTooltip = "Events for clicks.\nOnClickBegin - When user's pointer goes down on this UIElement.\nOnClickEnd - When user's pointer goes up after a registered click down.";
        private static readonly string FocusFoldoutTooltip = "Events for focus.\nOnFocusBegin - When this UIElement becomes focused by the ManagedEventSystem.\nOnFocusEnd - When this UIElement goes out of focus by the ManagedEventSystem.";
        private static readonly string HoverFoldoutTooltip = "Events for hovers.\nOnHoverBegin - When user's pointer begins hovering this UIElement.\nOnHoverEnd - When user's pointer ends hovering this UIElement.";
        private static readonly string HoverClickedFoldoutTooltip = "Events for hovering during a registered click.\nOnHoverBeginClicked - When user's pointer begins hovering this UIElement while an ongoing click is occurring.\nOnHoverEndClicked - When user's pointer ends hovering this UIElement while an ongoing click is occuring.";
        private static readonly string EventsFoldoutTooltip = "UI and Input events for this UIElement.";
        private static readonly string NavigationFoldoutTooltip = "Navigation settings for this UIElement.";
        #endregion

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.AddStyleSheet(Resources.Load<StyleSheet>("ManagedUI"));

            var scriptField = new PropertyField(serializedObject.FindProperty("m_Script"))
            {
                enabledSelf = false
            };

            Foldout flags = CreateFlags();
            Foldout events = CreateEvents();
            Foldout navigation = CreateNavigation();

            root.Add(scriptField);
            root.Add(flags);
            root.Add(events);
            root.Add(navigation);

            return root;
        }

        private Foldout CreateFlags()
        {
            var enableOnAwakeField = new PropertyField(serializedObject.FindProperty("enableOnAwake"));
            var selectableField = new PropertyField(serializedObject.FindProperty("selectable"));
            var focusableField = new PropertyField(serializedObject.FindProperty("focusable"));
            var hoverableField = new PropertyField(serializedObject.FindProperty("hoverable"));
            var flagContainer = new VisualElement();

            flagContainer.AddToClassList("flagContainer");
            flagContainer.Add(enableOnAwakeField);
            flagContainer.Add(selectableField);
            flagContainer.Add(focusableField);
            flagContainer.Add(hoverableField);

            var flagFoldout = CreateBoundFoldout("Flags", "flagsFoldout", FlagsFoldoutTooltip, flagContainer);

            return flagFoldout;
        }

        private Foldout CreateEvents()
        {
            var onEnabledField = new PropertyField(serializedObject.FindProperty("onEnabled"));
            var onDisabledField = new PropertyField(serializedObject.FindProperty("onDisabled"));
            var onSelectBeginField = new PropertyField(serializedObject.FindProperty("onSelectBegin"));
            var onSelectEndField = new PropertyField(serializedObject.FindProperty("onSelectEnd"));
            var onClickBeginField = new PropertyField(serializedObject.FindProperty("onClickBegin"));
            var onClickEndField = new PropertyField(serializedObject.FindProperty("onClickEnd"));
            var onFocusBeginField = new PropertyField(serializedObject.FindProperty("onFocusBegin"));
            var onFocusEndField = new PropertyField(serializedObject.FindProperty("onFocusEnd"));
            var onHoverBeginField = new PropertyField(serializedObject.FindProperty("onHoverBegin"));
            var onHoverEndField = new PropertyField(serializedObject.FindProperty("onHoverEnd"));
            var onHoverBeginClickedField = new PropertyField(serializedObject.FindProperty("onHoverBeginClicked"));
            var onHoverEndClickedField = new PropertyField(serializedObject.FindProperty("onHoverEndClicked"));

            var activationFoldout = CreateBoundFoldout("Activation", "activationFoldout", ActivationFoldoutTooltip, onEnabledField, onDisabledField);
            var selectFoldout = CreateBoundFoldout("Select", "selectFoldout", SelectFoldoutTooltip, onSelectBeginField, onSelectEndField);
            var clickFoldout = CreateBoundFoldout("Click", "clickFoldout", ClickFoldoutTooltip, onClickBeginField, onClickEndField);
            var focusFoldout = CreateBoundFoldout("Focus", "focusFoldout", FocusFoldoutTooltip, onFocusBeginField, onFocusEndField);
            var hoverFoldout = CreateBoundFoldout("Hover", "hoverFoldout", HoverFoldoutTooltip, onHoverBeginField, onHoverEndField);
            var hoverClickedFoldout = CreateBoundFoldout("Hover Clicked", "hoverClickedFoldout", HoverClickedFoldoutTooltip, onHoverBeginClickedField, onHoverEndClickedField);
            var eventsFoldout = CreateBoundFoldout("Events", "eventsFoldout", EventsFoldoutTooltip, activationFoldout, selectFoldout, clickFoldout, focusFoldout, hoverFoldout, hoverClickedFoldout);

            return eventsFoldout;
        }

        private Foldout CreateNavigation()
        {
            var navigationField = new PropertyField(serializedObject.FindProperty("navigation"));
            var navigationFoldout = CreateBoundFoldout("Navigation", "navigationFoldout", NavigationFoldoutTooltip, navigationField);
            
            return navigationFoldout;
        }

        private Foldout CreateBoundFoldout(string name, string propertyName, string tooltip, params VisualElement[] children)
        {
            var foldout = new Foldout
            {
                text = name,
                value = serializedObject.FindProperty(propertyName).boolValue,
                tooltip = tooltip
            };

            foldout.BindProperty(serializedObject.FindProperty(propertyName));

            foldout.AddToClassList("foldout");

            foreach (var child in children)
                foldout.Add(child);
             
            return foldout;
        }
    }
}
