using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomEditor(typeof(ManagedWrapper), true)]
    public class ManagedWrapperEditor : UnityEditor.Editor
    {
        protected Component wrappedValue;

        protected virtual void OnEnable()
        {
            if (wrappedValue != null)
                return;

            var wrapper = target as ManagedWrapper;

            if (wrapper == null)
                return;

            wrappedValue = wrapper.WrappedValue;

            EditorApplication.delayCall += SetHideFlags;
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= SetHideFlags;
        }

        private void SetHideFlags()
        {
            var wrappedValueSO = new SerializedObject(wrappedValue);
            wrappedValueSO.FindProperty("m_ObjectHideFlags").intValue = (int)HideFlags.HideInInspector;
            wrappedValueSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wrappedValueSO.targetObject);
        }

        protected virtual void OnDestroy()
        {
            if (target == null && wrappedValue != null)
            {
                var wrappers = wrappedValue.GetComponents<ManagedWrapper>();

                foreach (var wrapper in wrappers)
                {
                    if (wrapper.WrappedValue == wrappedValue)
                        return;
                }

                if (Application.isPlaying)
                    Destroy(wrappedValue);
                else
                    DestroyImmediate(wrappedValue);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var defaultFields = VisualElementUtil.CreateDefaultFields(serializedObject);
            var wrappedValueSO = new SerializedObject(wrappedValue);

            var wrappedFoldout = new Foldout
            {
                text = $"Wrapped {wrappedValue.GetType().Name} Settings",
                value = false
            };
            wrappedFoldout.AddStyleSheet(ShearsStyles.InspectorStyles);
            wrappedFoldout.AddToClassList(ShearsStyles.DarkFoldoutClass);

            var wrappedFields = VisualElementUtil.CreateDefaultFields(wrappedValueSO);
            wrappedFoldout.Add(wrappedFields);

            root.AddAll(defaultFields, wrappedFoldout);

            return root;
        }

        public override void OnInspectorGUI()
        {
            VisualElementUtil.CreateDefaultFieldsIMGUI(serializedObject);
        }
    }
}
