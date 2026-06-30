using System;
using System.Linq;
using Shears.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.DataManagement.Editor
{
    [CustomEditor(typeof(DataItem<>), true)]
    public class DataItemEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var defaultFields = VisualElementEditorUtil.CreateDefaultFields(serializedObject);

            var dataType = target.GetType().BaseType.GetGenericArguments()[0];
            var selectedTypeProp = serializedObject.FindProperty("selectedType");

            var header = VisualElementEditorUtil.CreateHeader("Type Selection");
            header.style.marginBottom = 0;
            var selector = TypeSelector.CreateInheritanceSelector(dataType, isSearchable: true);
            selector.BindProperty(selectedTypeProp);

            selector.TypeChanged += OnTypeChanged;

            root.AddAll(defaultFields, header, selector);

            return root;
        }

        private void OnTypeChanged(SerializableType type)
        {
            var blueprintProp = serializedObject.FindProperty("blueprint");

            if ((type is null || !type.IsValid()))
                blueprintProp.boxedValue = null;
            else
                blueprintProp.boxedValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
