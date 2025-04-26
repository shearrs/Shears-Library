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

            var flagFoldout = CreateBoundFoldout("Flags", "flagsFoldout", flagContainer);

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

            var activationFoldout = CreateBoundFoldout("Activation", "activationFoldout", onEnabledField, onDisabledField);
            var selectFoldout = CreateBoundFoldout("Select", "selectFoldout", onSelectBeginField, onSelectEndField);
            var clickFoldout = CreateBoundFoldout("Click", "clickFoldout", onClickBeginField, onClickEndField);
            var focusFoldout = CreateBoundFoldout("Focus", "focusFoldout", onFocusBeginField, onFocusEndField);
            var hoverFoldout = CreateBoundFoldout("Hover", "hoverFoldout", onHoverBeginField, onHoverEndField);
            var hoverClickedFoldout = CreateBoundFoldout("Hover Clicked", "hoverClickedFoldout", onHoverBeginClickedField, onHoverEndClickedField);
            var eventsFoldout = CreateBoundFoldout("Events", "eventsFoldout", activationFoldout, selectFoldout, clickFoldout, focusFoldout, hoverFoldout, hoverClickedFoldout);

            return eventsFoldout;
        }

        private Foldout CreateNavigation()
        {
            var navigationField = new PropertyField(serializedObject.FindProperty("navigation"));
            var navigationFoldout = CreateBoundFoldout("Navigation", "navigationFoldout", navigationField);
            
            return navigationFoldout;
        }

        private Foldout CreateBoundFoldout(string name, string propertyName, params VisualElement[] children)
        {
            var foldout = new Foldout()
            {
                text = name,
                value = serializedObject.FindProperty(propertyName).boolValue
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                serializedObject.FindProperty(propertyName).boolValue = evt.newValue;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            });

            foldout.AddToClassList("foldout");

            foreach (var child in children)
                foldout.Add(child);
             
            return foldout;
        }
    }
}
