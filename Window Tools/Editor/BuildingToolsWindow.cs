using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor.WindowTools
{
    public class BuildingToolsWindow : EditorWindow
    {
        private VisualElement root;

        [MenuItem("Shears Library/Building Tools")]
        private static void Open()
        {
            var window = GetWindow<BuildingToolsWindow>("Shears Building Tools");
            window.maxSize = new Vector2(600, 500);

            window.Show();
        }

        public void CreateGUI()
        {
            root = new VisualElement();
            root.AddStyleSheet(WindowToolsStyles.WindowToolsStyleSheet);
            root.AddToClassList(WindowToolsStyles.BuildingToolsClass);

            root.Add(new RandomRotateField());
            root.Add(new RandomScaleField());

            rootVisualElement.Add(root);
        }
    }
}
