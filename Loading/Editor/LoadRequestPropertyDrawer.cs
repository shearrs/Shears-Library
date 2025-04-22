using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Loading
{
    [CustomPropertyDrawer(typeof(LoadRequest))]
    public class LoadRequestPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var loadingScreenField = new PropertyField(property.FindPropertyRelative("opensLoadingScreen"));
            var pausesGameField = new PropertyField(property.FindPropertyRelative("pausesGame"));
            var actionsField = new PropertyField(property.FindPropertyRelative("actions"));

            container.Add(loadingScreenField);
            container.Add(pausesGameField);
            container.Add(actionsField);

            return container;
        }
    }
}
