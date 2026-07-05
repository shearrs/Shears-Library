using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(FoldoutAttribute))]
    public class FoldoutPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var attribute = this.attribute as FoldoutAttribute;
            VisualElement fields;

            if (!attribute.Show)
                fields = VisualElementEditorUtil.CreateDefaultFields(property);
            else
            {
                var foldout = new Foldout();
                var defaultFields = VisualElementEditorUtil.CreateDefaultFields(property);

                foldout.AddAll(defaultFields);

                fields = foldout;
            }

            return fields;
        }
    }
}
