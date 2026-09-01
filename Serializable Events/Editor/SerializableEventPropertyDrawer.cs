using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Events.Editor
{
    [CustomPropertyDrawer(typeof(SerializableEvent))]
    public class SerializableEventPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement { name = nameof(SerializableEvent).PascalSpace() };

            root.SetAllMargins(4.0f, 0.0f, 4.0f, 0.0f);
            root.SetAllPadding(0.0f, 0.0f, 8.0f, 0.0f);
            root.SetAllBorderRadii(4.0f);
            root.SetAllBorderWidths(1.0f);
            root.SetAllBorderColors(new(0.188f, 0.188f, 0.188f));
            root.style.backgroundColor = new Color(0.243f, 0.243f, 0.243f);

            var headerContainer = new VisualElement();
            headerContainer.SetAllPadding(4);
            headerContainer.style.width = Length.Percent(100);
            headerContainer.SetAllBorderRadii(4.0f, 4.0f, 0.0f, 0.0f);
            headerContainer.style.borderBottomColor = new Color(0.188f, 0.188f, 0.188f);
            headerContainer.style.borderBottomWidth = 1.0f;
            headerContainer.style.backgroundColor = new Color(0.318f, 0.318f, 0.318f);
            headerContainer.style.marginBottom = 4.0f;

            var headerField = new Label(property.displayName);
            headerField.style.unityTextAlign = TextAnchor.MiddleLeft;
            headerContainer.Add(headerField);

            var listenersProp = property.FindPropertyRelative("listeners");
            var listenersField = new PropertyField(listenersProp);
            listenersField.SetAllMargins(0.0f, 8.0f, 0.0f, 16.0f);

            root.AddAll(headerContainer, listenersField);

            return root;
        }
    }
}
