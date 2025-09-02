using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    public static class ShearsStyles
    {
        private const string StyleSheetPath = "Shears Library/Common Style Sheets";

        public static StyleSheet InspectorStyles => Resources.Load<StyleSheet>($"{StyleSheetPath}/Inspector");

        public const string HeaderClass = "header";
        public const string DarkContainerClass = "darkContainer";
        public const string DarkFoldoutClass = "darkFoldout";
    }
}
