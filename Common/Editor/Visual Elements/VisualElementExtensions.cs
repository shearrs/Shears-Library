using UnityEngine;
using UnityEngine.UIElements;

namespace Shears
{
    public static class VisualElementExtensions
    {
        public static void AddStyleSheet(this VisualElement element, StyleSheet styleSheet)
        {
            element.styleSheets.Add(styleSheet);
        }
    }
}
