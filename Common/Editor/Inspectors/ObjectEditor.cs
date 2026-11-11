using System;
using System.Collections.Generic;
using System.Reflection;
using KB.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomEditor(typeof(UnityEngine.Object), true, isFallback = true), CanEditMultipleObjects]
    public class ObjectEditor : UnityEditor.Editor
    {
        private Component _wrappedValue;

        private void OnEnable()
        {
            if (target == null)
                return;

            var targetType = target.GetType();

            if (TypeCache.GetTypesWithAttribute<WrapperAttribute>().Contains(targetType))
            {
                var attribute = targetType.GetCustomAttribute<WrapperAttribute>();
                var targetComponent = target as Component;
                _wrappedValue = targetComponent.GetComponent(attribute.WrappedType);

                WrapperSerializer.HideWrappedValue(target);
            }
            else
                _wrappedValue = null;
        }

        private void OnDestroy()
        {
            if (_wrappedValue != null)
                WrapperSerializer.OnDestroy(target, _wrappedValue);
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (TrySerializeInterfaces(out var inspector))
                return inspector;
            else if (TrySerializeWrapper(out inspector))
                return inspector;
            else
            {
                inspector = new VisualElement();

                InspectorElement.FillDefaultInspector(inspector, serializedObject, this);

                return inspector;
            }
        }

        private bool TrySerializeInterfaces(out VisualElement inspector)
        {
            if (target is IInterfaceSerializable)
            {
                inspector = InterfaceSerializer.Serialize(serializedObject);
                return true;
            }

            inspector = null;
            return false;
        }

        private bool TrySerializeWrapper(out VisualElement inspector)
        {
            var types = TypeCache.GetTypesWithAttribute<WrapperAttribute>();
            var targetType = target.GetType();

            if (types.Contains(targetType))
            {
                inspector = WrapperSerializer.Serialize(serializedObject, this);
                return true;
            }

            inspector = null;
            return false;
        }
    }
}
