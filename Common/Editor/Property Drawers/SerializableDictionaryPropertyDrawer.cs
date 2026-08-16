using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var entriesProp = property.FindPropertyRelative("entries");

            var listView = new ListView
            {
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showAddRemoveFooter = true,
                reorderable = true,
                showFoldoutHeader = true,
                headerTitle = property.displayName,
            };

            listView.BindProperty(entriesProp);

            //var label = new Label(property.displayName);

            root.AddAll(listView);

            return root;
        }
    }
}
