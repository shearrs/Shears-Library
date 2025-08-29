using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public static class VisualElementUtil
    {
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

        public static VisualElement CreateHeader(string text)
        {
            var header = new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 10,
                    marginBottom = 5
                }
            };

            return header;
        }

        public static void SetAllPadding(this VisualElement element, int padding)
        {
            element.style.paddingTop = padding;
            element.style.paddingBottom = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
        }
    }
}
