using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public static class VisualElementExtensions
    {
        public static void AddStyleSheet(this VisualElement element, StyleSheet styleSheet)
        {
            element.styleSheets.Add(styleSheet);
        }
    }
}
