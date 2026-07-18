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

        private readonly SerializableType defaultType;
        private readonly SerializableType searchType;
        private readonly SelectionType selectionType;
        private readonly Label label;
        private readonly Button button;
        private readonly bool isSearchable;
        private readonly GenericMenu genericMenu;
        private readonly TypeDropdown typeDropdown;
        private SerializedProperty boundProperty;

        public event Action<SerializableType> TypeChanged;

        public static TypeSelector CreateAttributeSelector<T>(
            SerializableType defaultType = null,
            bool isSearchable = false
        )
            where T : Attribute
        {
            var selector = new TypeSelector(
                SelectionType.Attribute,
                defaultType,
                typeof(T),
                isSearchable
            );

            return selector;
        }

        public static TypeSelector CreateInheritanceSelector<T>(
            SerializableType defaultType = null,
            bool isSearchable = false
        ) => CreateInheritanceSelector(typeof(T), defaultType, isSearchable);

        public static TypeSelector CreateInheritanceSelector(
            Type type,
            SerializableType defaultType = null,
            bool isSearchable = false
        )
        {
            var selector = new TypeSelector(
                SelectionType.Inheritance,
                defaultType,
                type,
                isSearchable
            );

            return selector;
        }

        private TypeSelector(
            SelectionType selectionType,
            SerializableType defaultType,
            SerializableType searchType,
            bool isSearchable
        )
        {
            this.selectionType = selectionType;
            this.defaultType = defaultType;
            this.searchType = searchType;
            this.isSearchable = isSearchable;

            var container = new VisualElement() { name = "Type Selector Container" };
            container.style.flexDirection = FlexDirection.Row;
            container.SetAllMargins(2, -2, 1, 3);
            container.style.overflow = Overflow.Hidden;
            container.style.fontSize = 12;

            label = new Label("Type");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.minWidth = 119.8f;
            label.style.width = Length.Percent(40);

            button = new Button(ShowContextMenu)
            {
                text =
                    (defaultType is null || !defaultType.IsValid())
                        ? "None"
                        : defaultType.PrettyName,
            };
            button.style.flexGrow = 1;
            button.style.marginLeft = StyleKeyword.Auto;

            if (!isSearchable)
                genericMenu = CreateUnsearchableMenu();
            else
                typeDropdown = CreateSearchableMenu();

            container.AddAll(label, button);
            Add(container);
        }

        public void BindProperty(SerializedProperty prop, bool initializeType = false)
        {
            if (prop.boxedValue is not SerializableType propValue)
            {
                Debug.LogError($"Property is not of type {nameof(SerializableType)}!");
                return;
            }

            boundProperty = prop;

            if (initializeType && (propValue is null || !propValue.IsValid()))
            {
                prop.boxedValue = defaultType;
                prop.serializedObject.ApplyModifiedProperties();
            }

            button.text =
                (propValue is null || !propValue.IsValid()) ? "None" : propValue.PrettyName;
            label.text = prop.displayName;
        }

        private void ShowContextMenu()
        {
            if (!isSearchable)
                genericMenu.ShowAsContext();
            else
                typeDropdown.Show(button.worldBound, 300);
        }

        private void TryAddMenuItem(GenericMenu menu, Type type)
        {
            var attribute = type.GetCustomAttribute<TypeSelectorItemAttribute>();
            string path =
                attribute != null ? attribute.MenuPath : StringUtil.PascalSpace(type.Name);

            menu.AddItem(new GUIContent(path), false, () => SetType(type));
        }

        private void SetType(SerializableType type)
        {
            if (boundProperty != null)
            {
                boundProperty.boxedValue = type;
                boundProperty.serializedObject.ApplyModifiedProperties();
            }

            button.text = (type is null || !type.IsValid()) ? "None" : type.PrettyName;

            TypeChanged?.Invoke(type);
        }

        private GenericMenu CreateUnsearchableMenu()
        {
            var menu = new GenericMenu();

            string defaultText =
                (defaultType is null || !defaultType.IsValid()) ? "None" : defaultType.PrettyName;

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

            return menu;
        }

        private TypeDropdown CreateSearchableMenu()
        {
            var menu = new TypeDropdown(
                selectionType,
                searchType,
                defaultType,
                SetType,
                new AdvancedDropdownState()
            );

            return menu;
        }

        private class TypeDropdown : AdvancedDropdown
        {
            private readonly SelectionType selectionType;
            private readonly SerializableType searchType;
            private readonly SerializableType defaultType;
            private readonly Action<SerializableType> setType;

            public TypeDropdown(
                SelectionType selectionType,
                SerializableType searchType,
                SerializableType defaultType,
                Action<SerializableType> setType,
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
                    (defaultType is null || !defaultType.IsValid())
                        ? "None"
                        : defaultType.PrettyName;

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
