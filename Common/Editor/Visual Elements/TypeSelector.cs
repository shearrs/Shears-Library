using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public class TypeSelector : VisualElement
    {
        public enum SelectionType
        {
            Attribute,
            Inheritance,
        }

        private readonly SerializableSystemType defaultType;
        private readonly SerializableSystemType searchType;
        private readonly SelectionType selectionType;
        private readonly Label label;
        private readonly Button button;
        private readonly bool isSearchable;
        private SerializedProperty boundProperty;

        public event Action<SerializableSystemType> TypeChanged;

        public static TypeSelector CreateAttributeSelector<T>(
            SerializableSystemType? defaultType = null,
            bool isSearchable = false
        )
            where T : Attribute
        {
            if (!defaultType.HasValue)
                defaultType = SerializableSystemType.Empty;

            var selector = new TypeSelector(
                SelectionType.Attribute,
                defaultType.Value,
                typeof(T),
                isSearchable
            );

            return selector;
        }

        public static TypeSelector CreateInheritanceSelector<T>(
            SerializableSystemType? defaultType = null,
            bool isSearchable = false
        ) => CreateInheritanceSelector(typeof(T), defaultType, isSearchable);

        public static TypeSelector CreateInheritanceSelector(
            Type type,
            SerializableSystemType? defaultType = null,
            bool isSearchable = false
        )
        {
            if (!defaultType.HasValue)
                defaultType = SerializableSystemType.Empty;

            var selector = new TypeSelector(
                SelectionType.Inheritance,
                defaultType.Value,
                type,
                isSearchable
            );

            return selector;
        }

        private TypeSelector(
            SelectionType selectionType,
            SerializableSystemType defaultType,
            SerializableSystemType searchType,
            bool isSearchable
        )
        {
            this.defaultType = defaultType;
            this.searchType = searchType;
            this.selectionType = selectionType;
            this.isSearchable = isSearchable;

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
                text =
                    defaultType == SerializableSystemType.Empty ? "None" : defaultType.PrettyName,
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
            if (!isSearchable)
                ShowUnsearchableMenu();
            else
                ShowSearchableMenu();
        }

        private void ShowUnsearchableMenu()
        {
            GenericMenu menu = new();

            string defaultText =
                defaultType == SerializableSystemType.Empty ? "None" : defaultType.PrettyName;

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

        private void ShowSearchableMenu()
        {
            var menu = new TypeDropdown(
                selectionType,
                searchType,
                defaultType,
                SetType,
                new AdvancedDropdownState()
            );

            menu.Show(button.worldBound, 300);
        }

        private void TryAddMenuItem(GenericMenu menu, Type type)
        {
            var attribute = type.GetCustomAttribute<TypeSelectorItemAttribute>();
            string path =
                attribute != null ? attribute.MenuPath : StringUtil.PascalSpace(type.Name);

            menu.AddItem(new GUIContent(path), false, () => SetType(type));
        }

        private void SetType(SerializableSystemType type)
        {
            if (boundProperty != null)
            {
                boundProperty.boxedValue = type;
                boundProperty.serializedObject.ApplyModifiedProperties();
            }

            button.text = type == SerializableSystemType.Empty ? "None" : type.PrettyName;

            TypeChanged?.Invoke(type);
        }

        private class TypeDropdown : AdvancedDropdown
        {
            private readonly SelectionType selectionType;
            private readonly SerializableSystemType searchType;
            private readonly SerializableSystemType defaultType;
            private readonly Action<SerializableSystemType> setType;

            public TypeDropdown(
                SelectionType selectionType,
                SerializableSystemType searchType,
                SerializableSystemType defaultType,
                Action<SerializableSystemType> setType,
                AdvancedDropdownState state
            )
                : base(state)
            {
                this.selectionType = selectionType;
                this.searchType = searchType;
                this.defaultType = defaultType;
                this.setType = setType;
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem($"{searchType.PrettyName}");
                TypeCache.TypeCollection types;

                string defaultText =
                    defaultType == SerializableSystemType.Empty ? "None" : defaultType.PrettyName;

                root.AddChild(new TypeItem(defaultText, () => setType(defaultType)));

                if (selectionType == SelectionType.Attribute)
                    types = TypeCache.GetTypesWithAttribute(searchType);
                else
                    types = TypeCache.GetTypesDerivedFrom(searchType);

                foreach (var type in types)
                {
                    if (type.IsAbstract)
                        continue;

                    var attribute = type.GetCustomAttribute<TypeSelectorItemAttribute>();
                    string path =
                        attribute != null ? attribute.MenuPath : StringUtil.PascalSpace(type.Name);

                    root.AddChild(new TypeItem(path, () => setType(type)));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is TypeItem typeItem)
                    typeItem.setType();
            }

            private class TypeItem : AdvancedDropdownItem
            {
                public readonly Action setType;

                public TypeItem(string name, Action setType)
                    : base(name)
                {
                    this.setType = setType;
                }
            }
        }
    }
}
