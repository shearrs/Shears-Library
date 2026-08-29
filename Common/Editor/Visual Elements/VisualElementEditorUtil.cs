using System.Linq;
using System.Reflection;
using Shears.Logging;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace Shears.Editor
{
    /// <summary>
    /// Utility class for creating and manipulating VisualElements in the Unity Editor.
    /// </summary>
    public static class VisualElementEditorUtil
    {
        /// <summary>
        /// Creates a simple element for testing layout and styling.
        /// </summary>
        /// <param name="width">The width of the element.</param>
        /// <param name="height">The height of the element.</param>
        /// <param name="color">The color of the element.</param>
        /// <returns>A square test element with absolute positioning.</returns>
        public static VisualElement CreateTestElement(
            float width = 50,
            float height = 50,
            Color color = default
        )
        {
            if (color == default)
                color = Color.white;

            var element = new VisualElement();
            element.style.width = width;
            element.style.height = height;
            element.style.backgroundColor = color;
            element.style.position = Position.Absolute;
            element.pickingMode = PickingMode.Ignore;
            element.name = "Test Element";

            return element;
        }

        /// <summary>
        /// Create a <see cref="Label"/> styled like a default UnityEditor header.
        /// </summary>
        /// <returns>A header <see cref="Label"/>.</returns>
        public static Label CreateHeader(string text)
        {
            var header = new Label(text)
            {
                name = "Header",
                style = { unityFontStyleAndWeight = FontStyle.Bold },
            };
            header.AddHeaderClass();
            header.AddBaseFieldLabelClass();

            return header;
        }

        /// <summary>
        /// Iterates through all visible properties of a <see cref="SerializedObject"/> and creates a <see cref="PropertyField"/> for each one.
        /// </summary>
        /// <param name="serializedObject">The <see cref="SerializedObject"/> to create fields for.</param>
        /// <param name="includeScript">Whether or not to draw the default script field.</param>
        /// <param name="excludedFields">Names of fields to exclude.</param>
        /// <returns>A <see cref="VisualElement"/> with all default <see cref="PropertyField"/>s for the passed <see cref="SerializedObject"/>.</returns>
        public static VisualElement CreateDefaultFields(
            SerializedObject serializedObject,
            bool includeScript = true,
            params string[] excludedFields
        )
        {
            var container = new VisualElement { name = "Default Fields" };

            var iterator = serializedObject.GetIterator();
            bool isNext = iterator.Next(true);

            if (!isNext)
                return container;

            while (iterator.NextVisible(false))
            {
                var prop = iterator.Copy();

                if (excludedFields.Contains(prop.name))
                    continue;

                if (prop.name == "m_Script" && !includeScript)
                    continue;

                var field = new PropertyField(prop) { name = prop.name };
                field.Bind(prop.serializedObject);

                if (prop.name == "m_Script")
                    field.SetEnabled(false);

                container.Add(field);
            }

            return container;
        }

        /// <summary>
        /// Create the default fields for a generic <see cref="SerializedProperty"/>.
        /// </summary>
        /// <param name="property">The property to create fields for.</param>
        /// <param name="excludedFields">Names of fields to exclude.</param>
        /// <returns>A container <see cref="VisualElement"/> with <see cref="PropertyField"/>s for each field.</returns>
        public static VisualElement CreateDefaultFields(
            SerializedProperty property,
            params string[] excludedFields
        )
        {
            var container = new VisualElement { name = "Default Fields" };

            var iterator = property.Copy();
            bool isNext = iterator.Next(true);

            if (!isNext)
                return container;

            var firstProp = iterator.Copy();

            if (firstProp.depth - 1 != property.depth)
                return container;

            var firstField = new PropertyField(firstProp) { name = firstProp.name };
            firstField.Bind(property.serializedObject);

            container.Add(firstField);

            while (iterator.NextVisible(false))
            {
                var prop = iterator.Copy();

                if (prop.depth < firstProp.depth)
                    break;

                if (excludedFields.Contains(prop.name))
                    continue;

                var field = new PropertyField(prop) { name = prop.name };
                field.Bind(prop.serializedObject);

                container.Add(field);
            }

            return container;
        }

        /// <summary>
        /// Create the default UnityEditor inspector script field.
        /// </summary>
        /// <param name="serializedObject">The target <see cref="SerializedObject"/>.</param>
        /// <returns>A <see cref="PropertyField"/> for the <see cref="Object"/> script field.</returns>
        public static PropertyField CreateScriptField(SerializedObject serializedObject)
        {
            var scriptProp = serializedObject.FindProperty("m_Script");
            var scriptField = new PropertyField(scriptProp) { name = "m_Script" };

            scriptField.Bind(serializedObject);
            scriptField.SetEnabled(false);

            return scriptField;
        }

        /// <summary>
        /// Overload for <see cref="AdvancedDropdown.Show(Rect)"/>. Clamps dropdown height to a max height.
        /// </summary>
        /// <param name="dropdown">The dropdown to show.</param>
        /// <param name="buttonRect">The rect of the dropdown's button.</param>
        /// <param name="maxHeight">The maximum height of dropdown menu.</param>
        public static void Show(this AdvancedDropdown dropdown, Rect buttonRect, float maxHeight)
        {
            dropdown.Show(buttonRect);

            var window = EditorWindow.focusedWindow;

            if (window == null)
            {
                SHLogger.LogWarning("EditorWindow.focusedWindow was null.");
                return;
            }

            if (!string.Equals(window.GetType().Namespace, typeof(AdvancedDropdown).Namespace))
            {
                SHLogger.LogWarning(
                    "EditorWindow.focusedWindow "
                        + EditorWindow.focusedWindow.GetType().FullName
                        + " was not in expected namespace."
                );
                return;
            }

            var rect = window.position;
            if (rect.height <= maxHeight)
                return;

            rect.height = maxHeight;
            window.minSize = rect.size;
            window.maxSize = rect.size;
            window.position = rect;
            window.ShowAsDropDown(GUIUtility.GUIToScreenRect(buttonRect), rect.size);
        }

        /// <summary>
        /// Set the cursor style when hovered.
        /// </summary>
        /// <param name="element">The element to set.</param>
        /// <param name="cursor">The style of the cursor when hovered.</param>
        public static void SetCursor(this VisualElement element, MouseCursor cursor)
        {
            object objCursor = new Cursor();
            var fields = typeof(Cursor).GetProperty(
                "defaultCursorId",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            fields.SetValue(objCursor, (int)cursor);

            element.style.cursor = new StyleCursor((Cursor)objCursor);
        }

        /// <summary>
        /// Adds the default UnityEditor styling for a <see cref="BaseField{T}"/>.
        /// </summary>
        /// <param name="element"></param>
        public static void AddBaseFieldClass(this VisualElement element)
        {
            element.AddToClassList(BaseField<Object>.ussClassName);
        }

        /// <summary>
        /// Adds the default UnityEditor styling for aligning a <see cref="BaseField{T}"/>.
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void AddBaseFieldAlignClass(this VisualElement element)
        {
            element.AddToClassList(BaseField<Object>.alignedFieldUssClassName);
        }

        /// <summary>
        /// Adds the default UnityEngine styling for a <see cref="PropertyField"/> <see cref="Label"/>.
        /// </summary>
        /// <param name="element"></param>
        public static void AddPropertyFieldLabelClass(this VisualElement element)
        {
            element.AddToClassList(PropertyField.labelUssClassName);
        }

        /// <summary>
        /// Adds the default UnityEditor styling for a <see cref="BaseField{T}"/> <see cref="Label"/>.
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void AddBaseFieldLabelClass(this VisualElement element)
        {
            element.AddToClassList(BaseField<Object>.labelUssClassName);
        }

        /// <summary>
        /// Adds the default UnityEditor styling for a <see cref="PropertyField"/>'s input.
        /// </summary>
        /// <param name="element">The element to style.</param>
        public static void AddPropertyFieldInputClass(this VisualElement element)
        {
            element.AddToClassList(PropertyField.inputUssClassName);
        }

        /// <summary>
        /// Adds the default UnityEditor styling for a header.
        /// </summary>
        /// <param name="label">The element to style.</param>
        public static void AddHeaderClass(this Label label)
        {
            label.AddToClassList("unity-header-drawer__label");
        }
    }
}
