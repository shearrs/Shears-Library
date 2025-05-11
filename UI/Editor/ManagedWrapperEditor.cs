using UnityEditor;
using UnityEngine;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedWrapper), true)]
    public class ManagedWrapperEditor : UnityEditor.Editor
    {
        private Component wrappedValue;

        protected virtual void OnEnable()
        {
            var wrapper = target as ManagedWrapper;

            wrappedValue = wrapper.WrappedValue;
            var wrappedValueSO = new SerializedObject(wrappedValue);

            wrappedValueSO.FindProperty("m_ObjectHideFlags").intValue = (int)HideFlags.HideInInspector;
            wrappedValueSO.ApplyModifiedPropertiesWithoutUndo();
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
                return;

            if (target == null && wrappedValue != null)
                DestroyImmediate(wrappedValue);
        }
    }
}
