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
    }
}
