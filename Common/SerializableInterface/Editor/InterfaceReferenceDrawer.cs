#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shears
{
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string ObjectValueFieldName = "objectValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var objectProperty = property.FindPropertyRelative(ObjectValueFieldName);
            var args = GetArguments(fieldInfo);

            EditorGUI.BeginProperty(position, label, property);

            var assignedObject = EditorGUI.ObjectField(position, label, objectProperty.objectReferenceValue, typeof(Object), true);

            if (assignedObject != null)
            {
                if (assignedObject is GameObject go)
                {
                    ValidateAndAssignObject(
                        objectProperty,
                        go.GetComponent(args.InterfaceType),
                        go.name,
                        args.InterfaceType.Name
                    );
                }
                else if (args.InterfaceType.IsAssignableFrom(assignedObject.GetType()))
                {
                    ValidateAndAssignObject(
                        objectProperty,
                        assignedObject,
                        args.InterfaceType.Name
                    );
                }
            }
            else
                objectProperty.objectReferenceValue = null;

            InterfaceReferenceUtil.OnGUI(position, objectProperty, label, args);
            EditorGUI.EndProperty();
        }

        #region Reading Types with Reflection
        private static InterfaceArgs GetArguments(FieldInfo info)
        {
            Type objectType;
            Type interfaceType;
            Type fieldType = info.FieldType;

            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }

            return new(objectType, interfaceType);
        }

        private static bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
        {
            objType = intfType = null;

            if (type?.IsGenericType != true) return false;

            Type genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(InterfaceReference<>))
                type = type.BaseType;

            if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
            {
                var types = type.GetGenericArguments();
                intfType = types[0];
                objType = types[1];

                return true;
            }

            return false;
        }

        private static void GetTypesFromList(Type type, out Type objType, out Type intfType)
        {
            objType = intfType = null;

            Type listInterface = type.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
        
            if (listInterface != null)
            {
                Type elementType = listInterface.GetGenericArguments()[0];
                TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
            }
        }
        #endregion

        private static void ValidateAndAssignObject(SerializedProperty property, Object target, string componentNameOrType, string interfaceName = null)
        {
            if (target != null)
                property.objectReferenceValue = target;
            else
            {
                Debug.LogWarning(
                    @$"The {(interfaceName != null
                    ? $"GameObject '{componentNameOrType}'"
                    : $"assigned object")} does not have a component that implements '{componentNameOrType}."
                );

                property.objectReferenceValue = null;
            }
        }
    }

    public readonly struct InterfaceArgs
    {
        public readonly Type ObjectType;
        public readonly Type InterfaceType;

        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)} needs to be of Type {typeof(Object)}");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");

            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
}
#endif