using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomEditor(typeof(Object), true)]
    public class ObjectEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // iterate over all visible fields
            // if a field has a FoldoutGroup attribute, add it to a foldout (mapped by name in a dictionary)
            return VisualElementUtil.CreateDefaultFields(serializedObject);
        }
    }
}
