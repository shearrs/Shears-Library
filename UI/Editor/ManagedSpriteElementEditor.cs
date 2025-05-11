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
        private ObjectField spriteField;
        private IntegerField sortingField;

        #region Initialization
        protected override void OnEnable()
        {
            base.OnEnable();

            Undo.undoRedoPerformed += SyncFields;

            var managedSpriteRenderer = target as ManagedSpriteElement;
            spriteRenderer = managedSpriteRenderer.TypedWrappedValue;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= SyncFields;
        }
        #endregion

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddStyleSheetFromPath("ManagedUI/ManagedUI");

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
            sortingField = CreateSortingOrderField();

            return CreateBoundFoldout("Sprite", "spriteFoldout", SpriteFoldoutTooltip, spriteField, sortingField);
        }

        private Foldout CreateFlags()
        {
            var enableOnAwakeField = new PropertyField(serializedObject.FindProperty("enableOnAwake"));
            var selectableField = new PropertyField(serializedObject.FindProperty("selectable"));
            var hoverableField = new PropertyField(serializedObject.FindProperty("hoverable"));
            var flagContainer = new VisualElement();

            flagContainer.AddToClassList("flagContainer");
            flagContainer.Add(enableOnAwakeField);
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

        private ObjectField CreateSpriteField()
        {
            var spriteField = new ObjectField("Sprite")
            {
                objectType = typeof(Sprite),
                value = spriteRenderer.sprite,
            };

            spriteField.RegisterValueChangedCallback(evt =>
            {
                if (spriteField.value == spriteRenderer.sprite)
                    return;

                Undo.RecordObject(spriteRenderer, "Change Sprite");

                spriteRenderer.sprite = evt.newValue as Sprite;

                EditorUtility.SetDirty(spriteRenderer);
            });

            return spriteField;
        }

        private IntegerField CreateSortingOrderField()
        {
            var sortingField = new IntegerField("Sorting Order")
            {
                value = spriteRenderer.sortingOrder,
            };

            sortingField.RegisterValueChangedCallback(evt =>
            {
                if (sortingField.value == spriteRenderer.sortingOrder)
                    return;

                Undo.RecordObject(spriteRenderer, "Change Sorting Order");

                spriteRenderer.sortingOrder = evt.newValue;

                EditorUtility.SetDirty(spriteRenderer);
            });

            return sortingField;
        }

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

        private void SyncFields()
        {
            if (spriteField.value != spriteRenderer.sprite)
                spriteField.value = spriteRenderer.sprite;
        }
    }
}
