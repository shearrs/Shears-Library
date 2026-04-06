using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public class TypeSelector : BindableElement, INotifyValueChanged<SerializableSystemType>
    {
        public enum SelectionType { Attribute, Inheritance }

        private readonly SerializableSystemType defaultType;
        private readonly SerializableSystemType searchType;
        private readonly SelectionType selectionType;
        private readonly Button button;
        private SerializableSystemType type;

        public SerializableSystemType value
        {
            get => type; 
            set
            {
                if (type == value)
                    return;

                var previous = type;
                SetValueWithoutNotify(value);

                using var evt = ChangeEvent<SerializableSystemType>.GetPooled(previous, type);
                evt.target = this;
                SendEvent(evt);
            }
        }

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
            type = defaultType;

            button = new Button(ShowContextMenu);
        }

        public void SetValueWithoutNotify(SerializableSystemType newValue)
        {
            value = newValue;
            button.text = value.PrettyName;
        }

        private void ShowContextMenu()
        {
            type = value;
            button.text = type.PrettyName;

            GenericMenu menu = new();

            menu.AddItem(new GUIContent(defaultType.PrettyName), false, () => SetType(defaultType));
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

        private void SetType(Type type)
        {
            value = type;
        }
    }
}
