using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        /// Creates a header similar to the Unity default for use in a custom inspector.
        /// </summary>
        /// <param name="text">The text in the header.</param>
        /// <returns>A header <see cref="VisualElement"/>.</returns>
        public static VisualElement CreateHeader(string text)
        {
            var header = new Label(text);

            header.AddStyleSheet(ShearsStyles.InspectorStyles);
            header.AddToClassList(ShearsStyles.HeaderClass);

            return header;
        }

        /// <summary>
        /// Shorthand function for setting all padding values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to pad.</param>
        /// <param name="padding">The amount of padding for all sides.</param>
        public static void SetAllPadding(this VisualElement element, int padding)
        {
            element.style.paddingTop = padding;
            element.style.paddingBottom = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
        }

        /// <summary>
        /// Shorthand function for setting all padding values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The elemend to pad.</param>
        /// <param name="paddingTop">The amount of top padding.</param>
        /// <param name="paddingRight">The amount of right padding.</param>
        /// <param name="paddingBottom">The amount of bottom padding.</param>
        /// <param name="paddingLeft">The amount of left padding.</param>
        public static void SetAllPadding(
            this VisualElement element,
            int paddingTop,
            int paddingRight,
            int paddingBottom,
            int paddingLeft
        )
        {
            element.style.paddingTop = paddingTop;
            element.style.paddingRight = paddingRight;
            element.style.paddingBottom = paddingBottom;
            element.style.paddingLeft = paddingLeft;
        }

        /// <summary>
        /// Shorthand function for setting all border values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set borders for.</param>
        /// <param name="border">The size of borders in pixels.</param>
        public static void SetAllBorders(this VisualElement element, int border)
        {
            element.style.borderTopWidth = border;
            element.style.borderBottomWidth = border;
            element.style.borderLeftWidth = border;
            element.style.borderRightWidth = border;
        }

        /// <summary>
        /// Shorthand function for setting all border colors on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set border colors for.</param>
        /// <param name="color">The color to make the border.</param>
        public static void SetAllBorderColors(this VisualElement element, Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }

        /// <summary>
        /// Shorthand function for setting all border radius on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set border radius for.</param>
        /// <param name="radius">The radius to make the border.</param>
        public static void SetAllBorderRadius(this VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        /// <summary>
        /// Shorthand function for setting all margin values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set margins for.</param>
        /// <param name="margin">The size of margins in pixels.</param>
        public static void SetAllMargins(this VisualElement element, int margin)
        {
            element.style.marginTop = margin;
            element.style.marginBottom = margin;
            element.style.marginLeft = margin;
            element.style.marginRight = margin;
        }

        /// <summary>
        /// Shorthand function for setting all margin values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set margins for.</param>
        /// <param name="margin">The size of margins in pixels.</param>
        public static void SetAllMargins(
            this VisualElement element,
            int marginTop,
            int marginRight,
            int marginBottom,
            int marginLeft
        )
        {
            element.style.marginTop = marginTop;
            element.style.marginRight = marginRight;
            element.style.marginBottom = marginBottom;
            element.style.marginLeft = marginLeft;
        }

        /// <summary>
        /// Iterates through all visible properties of a <see cref="SerializedObject"/> and creates a <see cref="PropertyField"/> for each one.
        /// </summary>
        /// <param name="serializedObject">The <see cref="SerializedObject"/> to create fields for.</param>
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

        public static VisualElement CreateDefaultFields(SerializedProperty serializedProperty)
        {
            var container = new VisualElement { name = "Default Fields" };

            var iterator = serializedProperty;
            bool isNext = iterator.Next(true);

            if (!isNext)
                return container;

            do
            {
                var prop = iterator.Copy();
                var field = new PropertyField(prop) { name = prop.name };
                field.Bind(prop.serializedObject);

                if (prop.name == "m_Script")
                    field.SetEnabled(false);

                container.Add(field);
            } while (iterator.NextVisible(false));

            return container;
        }

        public static void CreateDefaultFieldsIMGUI(
            SerializedObject serializedObject,
            bool includeScript = true
        )
        {
            var iterator = serializedObject.GetIterator();
            bool isNext = iterator.Next(true);

            if (!isNext)
                return;

            while (iterator.NextVisible(false))
            {
                var prop = iterator.Copy();

                if (prop.name == "m_Script" && !includeScript)
                    continue;

                if (prop.name == "m_Script")
                    EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.PropertyField(prop);

                if (prop.name == "m_Script")
                    EditorGUI.EndDisabledGroup();
            }
        }

        public static VisualElement CreateScriptField(SerializedObject serializedObject)
        {
            var scriptProp = serializedObject.FindProperty("m_Script");
            var scriptField = new PropertyField(scriptProp) { name = "m_Script" };

            scriptField.Bind(serializedObject);
            scriptField.SetEnabled(false);

            return scriptField;
        }

        // from user "SisusCo": https://discussions.unity.com/t/add-maximum-window-size-to-advanceddropdown-control/753671/3
        public static void Show(this AdvancedDropdown dropdown, Rect buttonRect, float maxHeight)
        {
            dropdown.Show(buttonRect);

            var window = EditorWindow.focusedWindow;

            if (window == null)
            {
                Debug.LogWarning("EditorWindow.focusedWindow was null.");
                return;
            }

            if (!string.Equals(window.GetType().Namespace, typeof(AdvancedDropdown).Namespace))
            {
                Debug.LogWarning(
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
    }
}
