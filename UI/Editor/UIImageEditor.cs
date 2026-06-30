using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(UIImage))]
    public class UIImageEditor : UnityEditor.Editor
    {
        private UnityEngine.UI.Image image;

        protected virtual void OnEnable()
        {
            if (image != null)
                return;

            var managedImage = target as UIImage;

            if (managedImage == null)
                return;

            image = managedImage.RawImage;

            EditorApplication.delayCall += SetHideFlags;
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= SetHideFlags;
        }

        private void SetHideFlags()
        {
            var imageSO = new SerializedObject(image);
            var flagsProp = imageSO.FindProperty("m_ObjectHideFlags");

            if (flagsProp.intValue == (int)HideFlags.HideInInspector)
                return;

            flagsProp.intValue = (int)HideFlags.HideInInspector;
            imageSO.ApplyModifiedPropertiesWithoutUndo();
        }

        protected virtual void OnDestroy()
        {
            if (target == null && image != null)
            {
                var wrappers = image.GetComponents<ManagedWrapper>();

                foreach (var wrapper in wrappers)
                {
                    if (wrapper.WrappedValue == image)
                        return;
                }

                if (Application.isPlaying)
                    Destroy(image);
                else
                    DestroyImmediate(image);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imageSO = new SerializedObject(image);

            var spriteProp = imageSO.FindProperty("m_Sprite");
            var colorProp = imageSO.FindProperty("m_Color");
            var baseColorProp = serializedObject.FindProperty("baseColor");
            var modulateProp = serializedObject.FindProperty("modulate");

            var scriptField = VisualElementEditorUtil.CreateScriptField(serializedObject);
            var spriteField = new ObjectField("Sprite") { objectType = typeof(Sprite) };
            spriteField.BindProperty(spriteProp);

            void updateColor(SerializedPropertyChangeEvent evt)
            {
                colorProp.colorValue = baseColorProp.colorValue * modulateProp.colorValue;

                imageSO.ApplyModifiedProperties();
            }

            var colorField = new PropertyField(baseColorProp);
            colorField.RegisterValueChangeCallback(updateColor);

            var modulateField = new PropertyField(modulateProp);
            modulateField.RegisterValueChangeCallback(updateColor);

            var imageContainer = new Foldout { text = "Wrapped Image Settings", value = false };
            imageContainer.AddStyleSheet(ShearsStyles.InspectorStyles);
            imageContainer.AddToClassList(ShearsStyles.DarkFoldoutClass);

            imageContainer.Add(VisualElementEditorUtil.CreateDefaultFields(imageSO));

            root.AddAll(scriptField, spriteField, colorField, modulateField, imageContainer);

            return root;
        }
    }
}
