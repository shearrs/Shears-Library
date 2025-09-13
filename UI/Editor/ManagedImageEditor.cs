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

        // wrap image fields
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var imageSO = new SerializedObject(image);

            var spriteProp = imageSO.FindProperty("m_Sprite");
            var colorProp = imageSO.FindProperty("m_Color");
            var modulateProp = serializedObject.FindProperty("modulate");

            var spriteField = new ObjectField("Sprite")
            {
                objectType = typeof(Sprite)
            };
            spriteField.BindProperty(spriteProp);

            var colorField = new ColorField("Color")
            {
                value = colorProp.colorValue
            };
            colorField.BindProperty(colorProp);

            var modulateField = new PropertyField(modulateProp);

            root.AddAll(spriteField, colorField, modulateField);

            return root;
        }
    }
}
