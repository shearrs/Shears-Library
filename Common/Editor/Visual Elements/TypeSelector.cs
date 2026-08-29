using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    /// <summary>
    /// <see cref="VisualElement"/> for selecting a <see cref="SerializableType"/>.<br/><br/>
    /// Creates a dropdown display to select all types that either:<br/>
    /// - Inherit from a specified <see cref="Type"/>, or<br/>
    /// - Have a specified <see cref="Attribute"/>
    /// </summary>
    public class TypeSelector : VisualElement
    {
        /// <summary>
        /// The default <see cref="Type"/> of the selector.
        /// </summary>
        private readonly SerializableType defaultType;

        /// <summary>
        /// The <see cref="Type"/> to search for.
        /// </summary>
        private readonly SerializableType searchType;

        /// <summary>
        /// The mode of the selector.
        /// </summary>
        private readonly TypeSelectionMode selectionType;

        /// <summary>
        /// The selector's property <see cref="Label"/>.
        /// </summary>
        private readonly Label label;

        /// <summary>
        /// The selector's dropdown <see cref="Button"/>.
        /// </summary>
        private readonly Button button;

        /// <summary>
        /// Whether or not the selector is searchable.
        /// </summary>
        private readonly bool isSearchable;

        /// <summary>
        /// The <see cref="GenericMenu"/> implementation (for non-searchable selector).
        /// </summary>
        private readonly GenericMenu genericMenu;

        /// <summary>
        /// The <see cref="TypeDropdown"/> implementation (for searchable selector).
        /// </summary>
        private readonly TypeDropdown typeDropdown;

        /// <summary>
        /// An optional predicate for deciding if a type should be included.
        /// </summary>
        private readonly Func<Type, bool> predicate;

        /// <summary>
        /// The bound <see cref="SerializableType"/> property.
        /// </summary>
        private SerializedProperty boundProperty;

        /// <summary>
        /// Event for when the selected type changes.
        /// </summary>
        public event Action<SerializableType> TypeChanged;

        /// <summary>
        /// Create a selector for selecting <see cref="Type"/>s that have a specified <see cref="Attribute"/>.
        /// </summary>
        /// <typeparam name="T">The relative <see cref="Type"/> to select.</typeparam>
        /// <param name="defaultType">The default <see cref="Type"/> for the menu to select.</param>
        /// <param name="isSearchable">Whether or not the menu will include a search bar.</param>
        /// <returns>A new <see cref="TypeSelector"/> instance.</returns>
        public static TypeSelector CreateAttributeSelector<T>(
            SerializableType defaultType = null,
            bool isSearchable = false,
            Func<Type, bool> predicate = null,
            string label = "Type"
        )
            where T : Attribute =>
            CreateAttributeSelector(typeof(T), defaultType, isSearchable, predicate, label);

        /// <inheritdoc cref="CreateAttributeSelector{T}(SerializableType, bool)"/>
        /// <param name="type">The relative type to select.</param>
        public static TypeSelector CreateAttributeSelector(
            SerializableType type,
            SerializableType defaultType = null,
            bool isSearchable = false,
            Func<Type, bool> predicate = null,
            string label = "Type"
        )
        {
            return new TypeSelector(
                TypeSelectionMode.Attribute,
                defaultType,
                type,
                isSearchable,
                predicate,
                label
            );
        }

        /// <summary>
        /// Create a selector for selecting <see cref="Type"/> that inherit from a specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The relative <see cref="Type"/> to select.</typeparam>
        /// <param name="defaultType">The default <see cref="Type"/> for the menu to select.<see cref=""/></param>
        /// <param name="isSearchable">Whether or not the menu will include a search bar.</param>
        /// <returns>A new <see cref="TypeSelector"/> instance.</returns>
        public static TypeSelector CreateInheritanceSelector<T>(
            SerializableType defaultType = null,
            bool isSearchable = false,
            Func<Type, bool> predicate = null,
            string label = "Type"
        ) => CreateInheritanceSelector(typeof(T), defaultType, isSearchable, predicate, label);

        /// <inheritdoc cref="CreateInheritanceSelector{T}(SerializableType, bool)"/>
        /// <param name="type">The relative <see cref="Type"/> to select.</param>
        public static TypeSelector CreateInheritanceSelector(
            SerializableType type,
            SerializableType defaultType = null,
            bool isSearchable = false,
            Func<Type, bool> predicate = null,
            string label = "Type"
        )
        {
            return new TypeSelector(
                TypeSelectionMode.Inheritance,
                defaultType,
                type,
                isSearchable,
                predicate,
                label
            );
        }

        private TypeSelector(
            TypeSelectionMode selectionType,
            SerializableType defaultType,
            SerializableType searchType,
            bool isSearchable,
            Func<Type, bool> predicate,
            string label
        )
        {
            this.selectionType = selectionType;
            this.defaultType = defaultType is null ? SerializableType.Empty : defaultType;
            this.searchType = searchType;
            this.isSearchable = isSearchable;
            this.predicate = predicate;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginTop = 2;
            container.style.marginRight = -2;
            container.style.marginBottom = 1;
            container.style.marginLeft = 3;
            container.style.overflow = Overflow.Hidden;
            container.style.fontSize = 12;

            if (!string.IsNullOrEmpty(label))
            {
                this.label = new(label);
                this.label.AddBaseFieldLabelClass();
                this.label.AddPropertyFieldLabelClass();
                container.Add(this.label);
            }

            button = new Button(ShowMenu)
            {
                text =
                    (this.defaultType is null || !this.defaultType.IsValid())
                        ? "None"
                        : this.defaultType.PrettyName,
            };
            button.style.flexGrow = 1;
            button.style.marginLeft = StyleKeyword.Auto;
            button.AddBaseFieldAlignClass();

            container.Add(button);
            Add(container);

            if (!isSearchable)
                genericMenu = CreateUnsearchableMenu();
            else
                typeDropdown = new(
                    selectionType,
                    searchType,
                    this.defaultType,
                    SetType,
                    this.predicate,
                    new()
                );
        }

        /// <summary>
        /// Bind a <see cref="SerializedProperty"/> to this menu. Only works with <see cref="SerializableType"/> properties. Causes the selected type to be applied to the property.
        /// </summary>
        /// <param name="prop">The property to bind.</param>
        /// <param name="initializeType">Whether or not the menu should initialize the property to be the default type.</param>
        public void BindProperty(SerializedProperty prop, bool initializeType = false)
        {
            if (prop.boxedValue is not SerializableType propValue)
            {
                Debug.LogError($"Property is not of type {nameof(SerializableType)}!");
                return;
            }

            boundProperty = prop;

            if (initializeType && propValue is null)
            {
                prop.boxedValue = defaultType;
                prop.serializedObject.ApplyModifiedProperties();
            }

            button.text =
                (propValue is null || !propValue.IsValid()) ? "None" : propValue.PrettyName;

            if (label != null)
                label.text = prop.displayName.PascalSpace();
        }

        /// <summary>
        /// Show the menu relative to the searchable setting.
        /// </summary>
        private void ShowMenu()
        {
            if (!isSearchable)
                genericMenu.ShowAsContext();
            else
                typeDropdown.Show(button.worldBound, 300);
        }

        /// <summary>
        /// Create the <see cref="GenericMenu"/> for unsearchable menus.
        /// </summary>
        /// <returns>An unsearchable menu.</returns>
        private GenericMenu CreateUnsearchableMenu()
        {
            var menu = new GenericMenu();

            string defaultText =
                (defaultType is null || !defaultType.IsValid()) ? "None" : defaultType.PrettyName;
            menu.AddItem(new GUIContent(defaultText), false, () => SetType(defaultType));

            TypeCache.TypeCollection types;

            if (selectionType == TypeSelectionMode.Attribute)
                types = TypeCache.GetTypesWithAttribute(searchType);
            else
                types = TypeCache.GetTypesDerivedFrom(searchType);

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (predicate != null && !predicate(type))
                    continue;

                TryAddMenuItem(menu, type);
            }

            return menu;
        }

        /// <summary>
        /// Shorthand method for adding an item to the unsearchable menu.
        /// </summary>
        /// <param name="menu">The menu to add to.</param>
        /// <param name="type">The <see cref="Type"/> to add.</param>
        private void TryAddMenuItem(GenericMenu menu, SerializableType type)
        {
            string path = type.PrettyName;

            menu.AddItem(new GUIContent(path), false, () => SetType(type));
        }

        /// <summary>
        /// Set the selected type..
        /// </summary>
        /// <param name="type">The selected <see cref="Type"/>.</param>
        public void SetType(SerializableType type)
        {
            if (boundProperty != null)
            {
                boundProperty.boxedValue = type;
                boundProperty.serializedObject.ApplyModifiedProperties();
            }

            button.text = (type is null | !type.IsValid()) ? "None" : type.PrettyName;

            TypeChanged?.Invoke(type);
        }
    }
}
