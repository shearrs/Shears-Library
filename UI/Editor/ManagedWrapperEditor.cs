using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
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
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var label = new Label("ManagedWrapper types should override Editor.CreateInspectorGUI!");

            root.Add(label);

            return root;
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
