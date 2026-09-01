using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceEntry))]
    public class InterfaceEntryPropertyDrawer : PropertyDrawer
    {
        private const string SETTINGS_ICON_PATH = "Shears Interface Serialization/Interface Icon";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement() { name = $"{nameof(InterfaceEntry)} Property Drawer" };

            var entry = property.boxedValue as InterfaceEntry;
            var typeProp = property.FindPropertyRelative("fieldType");
            var rawProp = property.FindPropertyRelative("rawObject");
            var unityObjectProp = property.FindPropertyRelative("unityObject");

            var dropdown = new TypeDropdown(
                TypeSelectionMode.Inheritance,
                entry.FieldType,
                typeof(UnityEngine.Object),
                type => OnTypeSelected(rawProp, unityObjectProp, type),
                t =>
                    !t.IsSubclassOf(typeof(MonoBehaviour))
                    && (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null),
                new()
            );

            var unityField = CreateObjectField(property, dropdown);
            VisualElement rawField = null;

            void updateField(SerializedProperty prop)
            {
                if (rawProp.managedReferenceValue == null)
                {
                    rawField?.RemoveFromHierarchy();
                    rawField = null;
                    root.Add(unityField);
                }
                else
                {
                    unityField.RemoveFromHierarchy();
                    rawField?.RemoveFromHierarchy();
                    rawField = CreateRawField(rawProp, dropdown, entry);
                    root.Add(rawField);
                }
            }

            root.TrackPropertyValue(property, updateField);
            updateField(rawProp);

            return root;
        }

        private void ShowDropdown(Func<Rect> bounds, TypeDropdown dropdown)
        {
            dropdown.Show(bounds(), 300);
        }

        private void OnTypeSelected(
            SerializedProperty rawProp,
            SerializedProperty unityObjectProp,
            SerializableType type
        )
        {
            var referenceValue = rawProp.managedReferenceValue;

            if (referenceValue != null && referenceValue.GetType() == (Type)type)
                return;

            if (type == typeof(UnityEngine.Object))
            {
                rawProp.managedReferenceValue = null;
                rawProp.serializedObject.ApplyModifiedProperties();
                return;
            }

            object newValue;

            if (type == SerializableType.Empty)
                newValue = null;
            else
                newValue = Activator.CreateInstance(type);

            rawProp.managedReferenceValue = newValue;
            unityObjectProp.objectReferenceValue = null;
            rawProp.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(rawProp.serializedObject.targetObject);
        }

        private VisualElement CreateObjectField(SerializedProperty property, TypeDropdown dropdown)
        {
            var entry = property.boxedValue as InterfaceEntry;
            var unityObjectProp = property.FindPropertyRelative("unityObject");

            var container = new VisualElement()
            {
                name = "Object Field Container",
                style = { flexDirection = FlexDirection.Row },
            };
            var field = new ObjectField(entry.FieldName.PascalSpace())
            {
                objectType = entry.FieldType,
                style =
                {
                    flexGrow = 1,
                    flexBasis = 0,
                    marginLeft = 1,
                },
            };
            field.labelElement.AddPropertyFieldLabelClass();
            field.AddBaseFieldAlignClass();
            field.BindProperty(unityObjectProp);

            var icon = CreateIcon();
            icon.style.marginTop = 2;
            container.AddAll(icon, field);

            var input = field.hierarchy[1];
            var inputLabel = input.hierarchy[0][1];

            void valueChanged(SerializedProperty prop)
            {
                if (prop.objectReferenceValue == null)
                    inputLabel.AddToClassList("unity-object-field-display__label--value-null");
                else
                    inputLabel.RemoveFromClassList("unity-object-field-display__label--value-null");
            }

            field.TrackPropertyValue(unityObjectProp, valueChanged);
            field.RegisterCallback<AttachToPanelEvent>(_ => valueChanged(unityObjectProp));

            Rect boundsCallback()
            {
                var rect = icon.worldBound;
                rect.xMax += 200;

                return Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
            }

            icon.RegisterCallback<ClickEvent>(_ => ShowDropdown(boundsCallback, dropdown));

            return container;
        }

        private VisualElement CreateRawField(
            SerializedProperty rawProp,
            TypeDropdown dropdown,
            InterfaceEntry entry
        )
        {
            var container = new VisualElement()
            {
                name = "Raw Field Container",
                style = { flexDirection = FlexDirection.Row, width = StyleKeyword.Auto },
            };
            var icon = CreateIcon();
            container.Add(icon);

            var propertyField = new PropertyField(rawProp)
            {
                label = entry.FieldName.PascalSpace(),
                style = { marginLeft = 30, flexGrow = 1 },
            };
            var foldout = new Foldout()
            {
                name = "Interface Serializer Foldout",
                text = entry.FieldName.PascalSpace(),
                style = { marginLeft = 30, flexGrow = 1 },
            };

            VisualElement currentField = null;

            void updateVisibleField(SerializedProperty property)
            {
                currentField?.RemoveFromHierarchy();

                if (property.managedReferenceValue == null)
                    return;

                currentField?.RemoveFromHierarchy();

                var value = property.managedReferenceValue;

                if (value is IInterfaceSerializer)
                {
                    foldout.Clear();
                    var fields = IInterfaceSerializerEditor.SerializeFields(property);

                    foldout.Add(fields);
                    currentField = foldout;
                }
                else
                {
                    propertyField.BindProperty(rawProp);
                    currentField = propertyField;
                }

                container.Add(currentField);

                icon.style.position = Position.Absolute;
                icon.style.top = 0;
                icon.style.right = StyleKeyword.Auto;
                icon.style.bottom = StyleKeyword.Auto;
                icon.style.left = 0;
            }

            container.TrackPropertyValue(rawProp, updateVisibleField);
            container.RegisterCallback<AttachToPanelEvent>(_ => updateVisibleField(rawProp));

            Rect boundsCallback()
            {
                var rect = icon.worldBound;
                rect.xMax += 200;

                return Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
            }

            icon.RegisterCallback<ClickEvent>(_ => ShowDropdown(boundsCallback, dropdown));

            return container;
        }

        private Image CreateIcon()
        {
            var settingsTexture = Resources.Load<Texture2D>(SETTINGS_ICON_PATH);
            var icon = new Image()
            {
                name = "Serialized Interface Icon",
                image = settingsTexture,
                style =
                {
                    height = StyleKeyword.Auto,
                    aspectRatio = 1,
                    alignSelf = Align.Center,
                    flexShrink = 0,
                },
            };

            void hover(PointerOverEvent _)
            {
                icon.tintColor = EditorStyles.label.focused.textColor;
            }

            void unhover(PointerOutEvent _)
            {
                icon.tintColor = Color.white;
            }

            icon.RegisterCallback<PointerOverEvent>(hover);
            icon.RegisterCallback<PointerOutEvent>(unhover);
            icon.SetCursor(MouseCursor.Link);

            return icon;
        }
    }
}
