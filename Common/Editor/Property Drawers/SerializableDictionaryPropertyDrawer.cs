using System.Collections.Generic;
using System.Text;
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
            var invalidEntriesProp = property.FindPropertyRelative("invalidEntries");

            var entryListView = new ListView
            {
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBoundCollectionSize = false,
                showAddRemoveFooter = true,
            };

            entryListView.BindProperty(entriesProp);

            var invalidHelpBox = new HelpBox("", HelpBoxMessageType.Warning)
            {
                name = "Invalid Entries HelpBox",
            };

            void updateInvalidEntries(SerializedProperty property)
            {
                if (invalidEntriesProp.arraySize == 0)
                {
                    invalidHelpBox.style.display = DisplayStyle.None;
                    return;
                }
                else
                    invalidHelpBox.style.display = DisplayStyle.Flex;

                CollectionUtil.GetPooled(out List<string> duplicateMessages);

                for (int i = 0; i < invalidEntriesProp.arraySize; i++)
                {
                    var invalid = invalidEntriesProp.GetArrayElementAtIndex(i);
                    var invalidKey = invalid.FindPropertyRelative("key").boxedValue;
                    var invalidValue = invalid.FindPropertyRelative("value").boxedValue;

                    string keyString = invalidKey == null ? "Null" : invalidKey.ToString();
                    string valueString = invalidValue == null ? "Null" : invalidValue.ToString();

                    duplicateMessages.Add($"Key: {keyString}, Value: {valueString}");
                }

                var builder = new StringBuilder();
                builder.AppendLine("Warning: Duplicate entries detected, they will be ignored.");

                foreach (var message in duplicateMessages)
                {
                    builder.Append("- ");
                    builder.AppendLine(message);
                }

                CollectionUtil.ReleasePooled(duplicateMessages);

                invalidHelpBox.text = builder.ToString();
            }

            invalidHelpBox.TrackPropertyValue(property, updateInvalidEntries);
            updateInvalidEntries(property);

            var label = new Label(property.displayName);

            root.Add(label);
            root.Add(invalidHelpBox);
            root.Add(entryListView);

            return root;
        }
    }
}
