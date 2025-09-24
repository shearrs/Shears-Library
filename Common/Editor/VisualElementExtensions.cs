using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    /// <summary>
    /// Extension methods for <see cref="VisualElement"/>s
    /// </summary>
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Adds a <see cref="StyleSheet"/> to a <see cref="VisualElement"/>'s stylesheets.
        /// </summary>
        /// <param name="element">The element to add a <see cref="StyleSheet"/> to.</param>
        /// <param name="styleSheet">The <see cref="StyleSheet"/> to add.</param>
        public static void AddStyleSheet(this VisualElement element, StyleSheet styleSheet)
        {
            element.styleSheets.Add(styleSheet);
        }
    }
}
