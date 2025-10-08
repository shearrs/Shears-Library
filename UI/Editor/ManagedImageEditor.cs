using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedImage))]
    public class ManagedImageEditor : UnityEditor.Editor
    {
        private UnityEngine.UI.Image image;

        public void OnEnable()
        {
            var managedImage = target as ManagedImage;
            image = managedImage.GetComponent<UnityEngine.UI.Image>();

            image.hideFlags = HideFlags.HideInInspector;
        }

        private void OnDisable()
        {
            var managedImage = target as ManagedImage;

            if (managedImage != null)
                return;

            DestroyImmediate(image);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imageSO = new SerializedObject(image);

            var spriteProp = imageSO.FindProperty("m_Sprite");
            var colorProp = imageSO.FindProperty("m_Color");
            var baseColorProp = serializedObject.FindProperty("baseColor");
            var modulateProp = serializedObject.FindProperty("modulate");

            var spriteField = new ObjectField("Sprite")
            {
                objectType = typeof(Sprite)
            };
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

            var imageContainer = new Foldout
            {
                text = "Wrapped Canvas Settings",
                value = false
            };
            imageContainer.AddStyleSheet(ShearsStyles.InspectorStyles);
            imageContainer.AddToClassList(ShearsStyles.DarkFoldoutClass);

            imageContainer.Add(VisualElementUtil.CreateDefaultFields(imageSO));

            root.AddAll(spriteField, colorField, modulateField, imageContainer);

            return root;
        }
    }
}
