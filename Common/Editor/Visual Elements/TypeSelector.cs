using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public class TypeSelector : VisualElement
    {
        public enum SelectionType { Attribute, Inheritance }

        private readonly SerializableSystemType defaultType;
        private readonly SerializableSystemType searchType;
        private readonly SelectionType selectionType;
        private readonly Label label;
        private readonly Button button;
        private SerializedProperty boundProperty;

        public static TypeSelector CreateAttributeSelector<T>(SerializableSystemType? defaultType = null) where T : Attribute
        {
            if (!defaultType.HasValue)
                defaultType = SerializableSystemType.Empty;

            var selector = new TypeSelector(SelectionType.Attribute, defaultType.Value, typeof(T));

            return selector;
        }

        public static TypeSelector CreateInheritanceSelector<T>(SerializableSystemType? defaultType = null)
        {
            if (!defaultType.HasValue)
                defaultType = SerializableSystemType.Empty;

            var selector = new TypeSelector(SelectionType.Inheritance, defaultType.Value, typeof(T));

            return selector;
        }

        private TypeSelector(SelectionType selectionType, SerializableSystemType defaultType, SerializableSystemType searchType)
        {
            this.defaultType = defaultType;
            this.searchType = searchType;
            this.selectionType = selectionType;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.SetAllMargins(1, -2, 1, 3);
            container.style.marginTop = 2;
            container.style.overflow = Overflow.Hidden;
            container.style.fontSize = 12;

            label = new Label("Type");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.minWidth = 119.8f;
            label.style.width = Length.Percent(40);

            button = new Button(ShowContextMenu)
            {
                text = defaultType == SerializableSystemType.Empty ? "None" : defaultType.PrettyName
            };
            button.style.flexGrow = 1;
            button.style.marginLeft = StyleKeyword.Auto;
            
            container.AddAll(label, button);
            Add(container);
        }

        public void BindProperty(SerializedProperty prop, bool initializeType = false)
        {
            if (prop.boxedValue is not SerializableSystemType propValue)
            {
                Debug.LogError($"Property is not of type {nameof(SerializableSystemType)}!");
                return;
            }

            boundProperty = prop;

            if (initializeType && propValue == SerializableSystemType.Empty)
            {
                prop.boxedValue = defaultType;
                prop.serializedObject.ApplyModifiedProperties();
            }

            button.text = propValue == SerializableSystemType.Empty ? "None" : propValue.PrettyName;
            label.text = prop.displayName;
        }

        private void ShowContextMenu()
        {
            GenericMenu menu = new();

            string defaultText = defaultType == SerializableSystemType.Empty ? "None" : defaultType.PrettyName;

            menu.AddItem(new GUIContent(defaultText), false, () => SetType(defaultType));
            TypeCache.TypeCollection types;

            if (selectionType == SelectionType.Attribute)
                types = TypeCache.GetTypesWithAttribute(searchType);
            else
                types = TypeCache.GetTypesDerivedFrom(searchType);

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                TryAddMenuItem(menu, type);
            }

            menu.ShowAsContext();
        }

        private void TryAddMenuItem(GenericMenu menu, Type type)
        {
            var attribute = type.GetCustomAttribute<TypeSelectorItemAttribute>();
            string path = attribute != null ? attribute.MenuPath : StringUtil.PascalSpace(type.Name);

            menu.AddItem(new GUIContent(path), false, () => SetType(type));
        }

        private void SetType(SerializableSystemType type)
        {
            boundProperty.boxedValue = type;
            boundProperty.serializedObject.ApplyModifiedProperties();

            button.text = type == SerializableSystemType.Empty ? "None" : type.PrettyName;
        }
    }
}
