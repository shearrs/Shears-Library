using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    /// <summary>
    /// Utility class for creating and manipulating VisualElements in the Unity Editor.
    /// </summary>
    public static class VisualElementUtil
    {
        /// <summary>
        /// Creates a simple element for testing layout and styling.
        /// </summary>
        /// <param name="width">The width of the element.</param>
        /// <param name="height">The height of the element.</param>
        /// <param name="color">The color of the element.</param>
        /// <returns>A square test element with absolute positioning.</returns>
        public static VisualElement CreateTestElement(float width = 50, float height = 50, Color color = default)
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
        /// Iterates through all visible properties of a <see cref="SerializedObject"/> and creates a <see cref="PropertyField"/> for each one.
        /// </summary>
        /// <param name="serializedObject">The <see cref="SerializedObject"/> to create fields for.</param>
        /// <returns>A <see cref="VisualElement"/> with all default <see cref="PropertyField"/>s for the passed <see cref="SerializedObject"/></returns>
        public static VisualElement CreateDefaultFields(SerializedObject serializedObject)
        {
            var container = new VisualElement
            {
                name = "Default Fields"
            };

            var iterator = serializedObject.GetIterator();
            iterator.Next(true);

            while (iterator.NextVisible(false))
            {
                var prop = iterator.Copy();
                var field = new PropertyField(prop);
                field.Bind(prop.serializedObject);

                container.Add(field);
            }

            return container;
        }
    }
}
