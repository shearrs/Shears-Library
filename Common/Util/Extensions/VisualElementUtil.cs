using Shears.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears
{
    public static class VisualElementUtil
    {
        /// <summary>
        /// Add a <see cref="StyleSheet"/> to a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to style.</param>
        /// <param name="styleSheet">The <see cref="StyleSheet"/> to add.</param>
        public static void AddStyleSheet(this VisualElement element, StyleSheet styleSheet)
        {
            if (styleSheet == null)
            {
                SHLogger.LogWarning($"Style sheet is null for {element.name}!");
                return;
            }

            element.styleSheets.Add(styleSheet);
        }

        /// <summary>
        /// Add a <see cref="StyleSheet"/> from a resource path.
        /// </summary>
        /// <param name="element">The element to style.</param>
        /// <param name="path">The resource path to load the <see cref="StyleSheet"/> from.</param>
        public static void AddStyleSheet(this VisualElement element, string path)
        {
            var styleSheet = Resources.Load<StyleSheet>(path);

            if (styleSheet == null)
            {
                SHLogger.LogWarning($"Style sheet for {element.name} not found at path: {path}");
                return;
            }

            element.AddStyleSheet(styleSheet);
        }

        /// <summary>
        /// Add a variable amount of <see cref="VisualElement"/>s to a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The parent to add to.</param>
        /// <param name="children">The children to add.</param>
        public static void AddAll(this VisualElement element, params VisualElement[] children)
        {
            foreach (var elem in children)
                element.Add(elem);
        }

        /// <summary>
        /// Shorthand function for setting all padding values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to pad.</param>
        /// <param name="padding">The amount of padding for all sides.</param>
        public static void SetAllPadding(this VisualElement element, StyleLength padding)
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
            StyleLength paddingTop,
            StyleLength paddingRight,
            StyleLength paddingBottom,
            StyleLength paddingLeft
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
        public static void SetAllBorderWidths(this VisualElement element, StyleFloat border)
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
        /// Shorthand function for setting all border radii on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set border radius for.</param>
        /// <param name="radius">The radius to make the border.</param>
        public static void SetAllBorderRadii(this VisualElement element, StyleLength radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        /// <inheritdoc cref="SetAllBorderRadii(VisualElement, StyleLength)"/>
        /// <param name="top">The top radius.</param>
        /// <param name="right">The right radius.</param>
        /// <param name="bottom">The bottom radius.</param>
        /// <param name="left">The left radius.</param>
        public static void SetAllBorderRadii(
            this VisualElement element,
            StyleLength top,
            StyleLength right,
            StyleLength bottom,
            StyleLength left
        )
        {
            element.style.borderTopLeftRadius = top;
            element.style.borderTopRightRadius = right;
            element.style.borderBottomLeftRadius = bottom;
            element.style.borderBottomRightRadius = left;
        }

        /// <summary>
        /// Shorthand function for setting all margin values on a <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="element">The element to set margins for.</param>
        /// <param name="margin">The size of margins in pixels.</param>
        public static void SetAllMargins(this VisualElement element, StyleLength margin)
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
            StyleLength marginTop,
            StyleLength marginRight,
            StyleLength marginBottom,
            StyleLength marginLeft
        )
        {
            element.style.marginTop = marginTop;
            element.style.marginRight = marginRight;
            element.style.marginBottom = marginBottom;
            element.style.marginLeft = marginLeft;
        }
    }
}
