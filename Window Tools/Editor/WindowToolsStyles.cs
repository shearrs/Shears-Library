using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor.WindowTools
{
    public static class WindowToolsStyles
    {
        public static StyleSheet WindowToolsStyleSheet => Resources.Load<StyleSheet>("Shears Window Tools/Window Tools");

        public const string BuildingToolsClass = "buildingTools";

        public const string RandomScaleFieldClass = "randomScaleField";
    }
}
