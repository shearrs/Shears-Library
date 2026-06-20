using UnityEngine;
using UnityEngine.UIElements;

namespace Shears
{
    public static class VisualElementUtil
    {
        public static void AddStyleSheet(this VisualElement element, StyleSheet styleSheet)
        {
            if (styleSheet == null)
            {
                Debug.LogWarning($"Style sheet is null for {element.name}!");
                return;
            }

            element.styleSheets.Add(styleSheet);
        }

        public static void AddStyleSheetFromPath(this VisualElement element, string path)
        {
            var styleSheet = Resources.Load<StyleSheet>(path);

            if (styleSheet == null)
            {
                Debug.LogWarning($"Style sheet for {element.name} not found at path: {path}");
                return;
            }

            element.AddStyleSheet(styleSheet);
        }

        public static void AddAll(this VisualElement element, params VisualElement[] elements)
        {
            foreach (var elem in elements)
                element.Add(elem);
        }
    }
}
