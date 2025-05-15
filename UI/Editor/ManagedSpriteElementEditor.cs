using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedSpriteElement))]
    public class ManagedSpriteElementEditor : ManagedWrapperEditor
    {
        #region Tooltips
        private static readonly string SpriteFoldoutTooltip = "Sprite settings for this SpriteElement.";
        private static readonly string FlagsFoldoutTooltip = "Status flags for the SpriteElement.";
        private static readonly string ActivationFoldoutTooltip = "Events for activation.\nOnEnabled - When 'Enable()' is called on this SpriteElement.\nOnDisabled - When 'Disable()' is called on this SpriteElement.";
        private static readonly string ClickFoldoutTooltip = "Events for clicks.\nOnClickBegin - When user's pointer goes down on this SpriteElement.\nOnClickEnd - When user's pointer goes up after a registered click down.";
        private static readonly string HoverFoldoutTooltip = "Events for hovers.\nOnHoverBegin - When user's pointer begins hovering this SpriteElement.\nOnHoverEnd - When user's pointer ends hovering this SpriteElement.";
        private static readonly string HoverClickedFoldoutTooltip = "Events for hovering during a registered click.\nOnHoverBeginClicked - When user's pointer begins hovering this SpriteElement while an ongoing click is occurring.\nOnHoverEndClicked - When user's pointer ends hovering this SpriteElement while an ongoing click is occuring.";
        private static readonly string EventsFoldoutTooltip = "Input events for this SpriteElement.";
        #endregion

        private SpriteRenderer spriteRenderer;
        private PropertyField baseColorField;
        private ObjectField spriteField;
        private IntegerField sortingField;

        #region Initialization
        protected override void OnEnable()
        {
            base.OnEnable();

            Undo.undoRedoPerformed += SyncFields;

            var managedSpriteRenderer = target as ManagedSpriteElement;
            spriteRenderer = managedSpriteRenderer.TypedWrappedValue;

            var wrappedValueSO = new SerializedObject(wrappedValue);
            wrappedValueSO.FindProperty("m_ObjectHideFlags").intValue = (int)HideFlags.HideInInspector;
            wrappedValueSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wrappedValueSO.targetObject);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= SyncFields;
        }
        #endregion

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddStyleSheetFromPath("ManagedElements/ManagedElements");

            var scriptField = new PropertyField(serializedObject.FindProperty("m_Script"))
            {
                enabledSelf = false
            };

            var spriteFoldout = CreateSpriteFoldout();
            var flagFoldout = CreateFlags();
            var eventsFoldout = CreateEvents();

            root.Add(scriptField);
            root.Add(spriteFoldout);
            root.Add(flagFoldout);
            root.Add(eventsFoldout);

            return root;
        }

        private Foldout CreateSpriteFoldout()
        {
            spriteField = CreateSpriteField();
            baseColorField = CreateBaseColorField();
            var currentColorField = CreateCurrentColorField();
            sortingField = CreateSortingOrderField();
            var spriteContainer = new VisualElement();

            spriteContainer.AddToClassList("flagContainer");
            spriteContainer.Add(spriteField);
            spriteContainer.Add(baseColorField);
            spriteContainer.Add(currentColorField);
            spriteContainer.Add(sortingField);

            return CreateBoundFoldout("Sprite", "spriteFoldout", SpriteFoldoutTooltip, spriteContainer);
        }

        private Foldout CreateFlags()
        {
            var awakeBehaviourField = new PropertyField(serializedObject.FindProperty("awakeBehaviour"));
            var isEnabledField = new PropertyField(serializedObject.FindProperty("isEnabled"));
            var selectableField = new PropertyField(serializedObject.FindProperty("selectable"));
            var hoverableField = new PropertyField(serializedObject.FindProperty("hoverable"));
            var flagContainer = new VisualElement();

            flagContainer.AddToClassList("flagContainer");
            flagContainer.Add(awakeBehaviourField);
            flagContainer.Add(isEnabledField);
            flagContainer.Add(selectableField);
            flagContainer.Add(hoverableField);

            var flagFoldout = CreateBoundFoldout("Flags", "flagsFoldout", FlagsFoldoutTooltip, flagContainer);

            return flagFoldout;
        }

        private Foldout CreateEvents()
        {
            var onEnabledField = new PropertyField(serializedObject.FindProperty("onEnabled"));
            var onDisabledField = new PropertyField(serializedObject.FindProperty("onDisabled"));
            var onClickBeginField = new PropertyField(serializedObject.FindProperty("onClickBegin"));
            var onClickEndField = new PropertyField(serializedObject.FindProperty("onClickEnd"));
            var onHoverBeginField = new PropertyField(serializedObject.FindProperty("onHoverBegin"));
            var onHoverEndField = new PropertyField(serializedObject.FindProperty("onHoverEnd"));
            var onHoverBeginClickedField = new PropertyField(serializedObject.FindProperty("onHoverBeginClicked"));
            var onHoverEndClickedField = new PropertyField(serializedObject.FindProperty("onHoverEndClicked"));

            var activationFoldout = CreateBoundFoldout("Activation", "activationFoldout", ActivationFoldoutTooltip, onEnabledField, onDisabledField);
            var clickFoldout = CreateBoundFoldout("Click", "clickFoldout", ClickFoldoutTooltip, onClickBeginField, onClickEndField);
            var hoverFoldout = CreateBoundFoldout("Hover", "hoverFoldout", HoverFoldoutTooltip, onHoverBeginField, onHoverEndField);
            var hoverClickedFoldout = CreateBoundFoldout("Hover Clicked", "hoverClickedFoldout", HoverClickedFoldoutTooltip, onHoverBeginClickedField, onHoverEndClickedField);
            var eventsFoldout = CreateBoundFoldout("Events", "eventsFoldout", EventsFoldoutTooltip, activationFoldout, clickFoldout, hoverFoldout, hoverClickedFoldout);

            return eventsFoldout;
        }

        #region Sprite Fields
        private ObjectField CreateSpriteField()
        {
            var spriteField = new ObjectField("Sprite")
            {
                objectType = typeof(Sprite),
                value = spriteRenderer.sprite,
            };

            BindField<ObjectField, UnityEngine.Object>(
                spriteField, 
                (value) => spriteRenderer.sprite = (Sprite)value, 
                () => spriteRenderer.sprite == spriteField.value, 
                "Change Sprite", spriteRenderer);

            return spriteField;
        }

        private PropertyField CreateBaseColorField()
        {
            var baseColorProp = serializedObject.FindProperty("baseColor");
            var colorField = new PropertyField(baseColorProp);

            colorField.RegisterValueChangeCallback(evt =>
            {
                if (Application.isPlaying)
                    return;

                spriteRenderer.color = baseColorProp.colorValue;
            });

            return colorField;
        }

        private ColorField CreateCurrentColorField()
        {
            var colorField = new ColorField("Current Color")
            {
                value = spriteRenderer.color,
                enabledSelf = false
            };

            colorField.schedule.Execute(() =>
            {
                if (spriteRenderer == null)
                    return;

                colorField.value = spriteRenderer.color;
                Repaint();
            }).Every(10);

            return colorField;
        }

        private IntegerField CreateSortingOrderField()
        {
            var sortingField = new IntegerField("Sorting Order")
            {
                value = spriteRenderer.sortingOrder,
            };

            BindField<IntegerField, int>(sortingField, 
                (value) => spriteRenderer.sortingOrder = value, 
                () => spriteRenderer.sortingOrder == sortingField.value,
                "Change Sorting Order", spriteRenderer);

            return sortingField;
        }

        private TField BindField<TField, TValue>(
            TField field,
            Action<TValue> setTargetValue, Func<bool> valuesAreEqual,
            string undoMessage, UnityEngine.Object dirtyTarget) 
                where TField : BaseField<TValue>
        {
            field.RegisterValueChangedCallback(evt =>
            {
                if (valuesAreEqual())
                    return;

                Undo.RecordObject(dirtyTarget, undoMessage);

                setTargetValue(evt.newValue);

                EditorUtility.SetDirty(dirtyTarget);
            });

            return field;
        }

        private void SyncFields()
        {
            if (spriteField.value != spriteRenderer.sprite)
                spriteField.value = spriteRenderer.sprite;

            if (sortingField.value != spriteRenderer.sortingOrder)
                sortingField.value = spriteRenderer.sortingOrder;
        }
        #endregion

        private Foldout CreateBoundFoldout(string foldoutName, string propertyName, string tooltip, params VisualElement[] children)
        {
            var foldout = new Foldout
            {
                text = foldoutName,
                value = serializedObject.FindProperty(propertyName).boolValue,
                tooltip = tooltip
            };

            foldout.BindProperty(serializedObject.FindProperty(propertyName));

            foldout.AddToClassList("foldout");

            foreach (var child in children)
                foldout.Add(child);

            return foldout;
        }
    }
}
