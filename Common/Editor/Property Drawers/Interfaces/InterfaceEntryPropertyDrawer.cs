using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    /// <summary>
    /// Property drawer for <see cref="InterfaceEntry"/>s. Draws either a Unity Object field or a raw object depending on selected type.
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceEntry))]
    public class InterfaceEntryPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// The resource path to load the interface icon from.
        /// </summary>
        private const string SETTINGS_ICON_PATH = "Shears Interface Serialization/Interface Icon";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement()
            {
                name = $"{nameof(InterfaceEntry)} Property Drawer",
                style = { marginLeft = 3 },
            };
            root.AddBaseFieldAlignClass();

            var entry = property.boxedValue as InterfaceEntry;
            var typeProp = property.FindPropertyRelative("fieldType");
            var rawProp = property.FindPropertyRelative("rawObject");
            var unityObjectProp = property.FindPropertyRelative("unityObject");

            // Create the type selector dropdown for selecting what concrete implementation to use.
            var dropdown = new TypeDropdown(
                TypeSelectionMode.Inheritance,
                entry.FieldType,
                typeof(UnityEngine.Object),
                type => OnTypeSelected(rawProp, unityObjectProp, type),
                t =>
                    !t.IsSubclassOf(typeof(MonoBehaviour))
                    && !t.IsSubclassOf(typeof(ScriptableObject))
                    && (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null),
                new()
            );

            // Create the Unity Object field persistently, the raw field is created when needed.
            var unityField = CreateObjectField(property, dropdown);
            VisualElement rawField = null;

            // Callback to swap out what field is currently drawn depending on the current value.
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

        /// <summary>
        /// Show the type dropdown.
        /// </summary>
        /// <param name="bounds">A callback to get bounds with.</param>
        /// <param name="dropdown">The dropdown to show.</param>
        private void ShowDropdown(Func<Rect> bounds, TypeDropdown dropdown)
        {
            dropdown.Show(bounds(), 300);
        }

        /// <summary>
        /// Callback when a type is selected in the <see cref="TypeDropdown"/>. Assigns the selected type value to the actual target property.
        /// </summary>
        /// <param name="rawProp">The raw C# object property.</param>
        /// <param name="unityObjectProp">The Unity Object property.</param>
        /// <param name="type">The selected type.</param>
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

        /// <summary>
        /// Create a Unity Object field.
        /// </summary>
        /// <param name="property">The target <see cref="InterfaceEntry"/> property.</param>
        /// <param name="dropdown">The <see cref="TypeDropdown"/>.</param>
        /// <returns>A <see cref="VisualElement"/> containing a Unity Object field.</returns>
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

        /// <summary>
        /// Create a serialized field for a raw C# object.
        /// </summary>
        /// <param name="rawProp">The raw object property.</param>
        /// <param name="dropdown">The <see cref="TypeDropdown"/>.</param>
        /// <param name="entry">The concrete entry for this property.</param>
        /// <returns>A <see cref="VisualElement"/> containing a raw C# object property field.</returns>
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
            container.AddBaseFieldAlignClass();
            var icon = CreateIcon();
            container.Add(icon);

            string fieldName = entry.FieldName.PascalSpace();
            string typeName = rawProp.managedReferenceValue.GetType().Name;
            string fieldLabel = $"{fieldName} <color=#8E8E8E>({typeName})</color>";

            var propertyField = new PropertyField()
            {
                label = fieldLabel,
                style = { marginLeft = 14, flexGrow = 1 },
                tooltip = $"{fieldName} is an interface field with concrete type '{typeName}'",
            };
            var foldout = new Foldout()
            {
                name = "Interface Serializer Foldout",
                text = fieldLabel,
                style = { marginLeft = 30, flexGrow = 1 },
                tooltip = $"{fieldName} is an interface field with concrete type '{typeName}'",
            };

            VisualElement currentField = null;

            void updateVisibleField(SerializedProperty property)
            {
                currentField?.RemoveFromHierarchy();

                if (property.managedReferenceValue == null)
                    return;

                var value = property.managedReferenceValue;

                if (value is IInterfaceSerializable)
                {
                    foldout.Clear();
                    var fields = InterfaceSerializer.SerializeFields(property);
                    fields.style.paddingBottom = 8;

                    foldout.Add(fields);
                    currentField = foldout;
                }
                else
                {
                    propertyField.BindProperty(property);
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

        /// <summary>
        /// Create a selectable interface icon for opening the <see cref="TypeSelector"/>.
        /// </summary>
        /// <returns>A selectable interface icon.</returns>
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
