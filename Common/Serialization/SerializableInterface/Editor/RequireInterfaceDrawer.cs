#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shears
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        private RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;
            
            EditorGUI.BeginProperty(position, label, property);

            if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
                DrawArrayField(position, property, label, requiredInterfaceType);
            else
                DrawInterfaceObjectField(position, property, label, requiredInterfaceType);

            EditorGUI.EndProperty();

            InterfaceArgs args = new(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);
            InterfaceReferenceUtil.OnGUI(position, property, label, args);
        }

        private void DrawArrayField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            Rect intRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.arraySize = EditorGUI.IntField(intRect, label.text + " Size", property.arraySize);

            float yOffset = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                Rect elementRect = new(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                
                DrawInterfaceObjectField(elementRect, element, new GUIContent($"Element {i}"), interfaceType);
                
                yOffset += EditorGUIUtility.singleLineHeight;
            }
        }

        private void DrawInterfaceObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType)
        {
            Object oldReference = property.objectReferenceValue;
            Object newReference = EditorGUI.ObjectField(position, label, oldReference, typeof(Object), true);

            if (newReference != null && newReference != oldReference)
                ValidateAndAssignObject(property, newReference, interfaceType);
            else if (newReference == null)
                property.objectReferenceValue = null;
        }

        private void ValidateAndAssignObject(SerializedProperty property, Object newReference, Type interfaceType)
        {
            if (newReference is GameObject go)
            {
                var component = go.GetComponent(interfaceType);

                if (component != null)
                {
                    property.objectReferenceValue = component;
                    return;
                }
            }
            else if (interfaceType.IsAssignableFrom(newReference.GetType()))
            {
                property.objectReferenceValue = newReference;
                return;
            }

            Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
            property.objectReferenceValue = null;
        }

        private Type GetTypeOrElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}
#endif