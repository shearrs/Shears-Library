using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Shears.Editor
{
    /// <summary>
    /// Dropdown definition for a searchable menu.
    /// </summary>
    public class TypeDropdown : AdvancedDropdown
    {
        private readonly Dictionary<string, Action> customItems = new();
        private readonly TypeSelectionMode selectionType;
        private readonly SerializableType searchType;
        private readonly SerializableType defaultType;
        private readonly Action<SerializableType> setType;
        private readonly Func<Type, bool> predicate;
        private readonly bool useDefaultType;

        public TypeDropdown(
            TypeSelectionMode selectionType,
            SerializableType searchType,
            SerializableType defaultType,
            Action<SerializableType> setType,
            Func<Type, bool> predicate,
            AdvancedDropdownState state
        )
            : base(state)
        {
            this.selectionType = selectionType;
            this.searchType = searchType;
            this.defaultType = defaultType;
            this.setType = setType;
            this.predicate = predicate;
            useDefaultType = true;
        }

        public TypeDropdown(
            TypeSelectionMode selectionType,
            SerializableType searchType,
            Action<SerializableType> setType,
            Func<Type, bool> predicate,
            AdvancedDropdownState state
        )
            : base(state)
        {
            this.selectionType = selectionType;
            this.searchType = searchType;
            defaultType = null;
            this.setType = setType;
            this.predicate = predicate;
            useDefaultType = false;
        }

        public void AddCustomItem(string name, Action selectedCallback)
        {
            customItems[name] = selectedCallback;
        }

        public void RemoveCustomItem(string name)
        {
            customItems.Remove(name);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem($"{searchType.PrettyName}");

            if (useDefaultType)
            {
                string defaultText =
                    (defaultType is null || !defaultType.IsValid())
                        ? "None"
                        : defaultType.PrettyName;
                root.AddChild(new TypeItem(defaultText, () => setType(defaultType)));
            }

            TypeCache.TypeCollection types;

            if (selectionType == TypeSelectionMode.Attribute)
                types = TypeCache.GetTypesWithAttribute(searchType);
            else
                types = TypeCache.GetTypesDerivedFrom(searchType);

            foreach (var (name, action) in customItems)
            {
                var item = new CustomItem(name, action);

                root.AddChild(item);
            }

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (predicate != null && !predicate(type))
                    continue;

                string path = type.Name.PascalSpace();

                root.AddChild(new TypeItem(path, () => setType(type)));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem typeItem)
                typeItem.SetType();
            else if (item is CustomItem customItem)
                customItem.Selected();
        }

        private class CustomItem : AdvancedDropdownItem
        {
            public readonly Action Selected;

            public CustomItem(string name, Action selected)
                : base(name)
            {
                Selected = selected;
            }
        }

        private class TypeItem : AdvancedDropdownItem
        {
            public readonly Action SetType;

            public TypeItem(string name, Action setType)
                : base(name)
            {
                SetType = setType;
            }
        }
    }
}
