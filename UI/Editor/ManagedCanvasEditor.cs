using Shears.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedCanvas))]
    public class ManagedCanvasEditor : ManagedWrapperEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var defaultFields = VisualElementUtil.CreateDefaultFields(serializedObject);
            var canvasContainer = new Foldout
            {
                text = "Wrapped Canvas Settings"
            };
            canvasContainer.AddStyleSheet(ShearsStyles.InspectorStyles);
            canvasContainer.AddToClassList(ShearsStyles.DarkFoldoutClass);

            var canvasSO = new SerializedObject(wrappedValue);
            canvasContainer.Add(VisualElementUtil.CreateDefaultFields(canvasSO));

            root.AddAll(defaultFields, canvasContainer);

            return root;
        }
    }
}
