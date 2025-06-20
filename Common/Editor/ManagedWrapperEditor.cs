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

            wrappedValue = wrapper.WrappedValue;

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
    }
}
