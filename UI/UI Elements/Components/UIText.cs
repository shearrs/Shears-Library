using TMPro;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class UIText : UIElement
    {
        [SerializeField]
        private Color baseColor = Color.white;

        [SerializeField]
        private Color modulate = Color.white;

        private TextMeshPro textMesh;

        public TextMeshPro TextMesh
        {
            get
            {
                if (textMesh == null)
                    textMesh = GetComponent<TextMeshPro>();

                return textMesh;
            }
        }

        public override Color BaseColor
        {
            get => baseColor;
            set
            {
                baseColor = value;
                textMesh.color = Modulate * baseColor;
            }
        }

        public override Color Modulate
        {
            get => modulate;
            set
            {
                modulate = value;
                textMesh.color = Modulate * baseColor;
            }
        }

        public string Text
        {
            get => textMesh.text;
            set => textMesh.text = value;
        }

        private void Reset()
        {
            var text = GetComponent<TextMeshPro>();
            baseColor = text.color;
        }

        private void OnValidate()
        {
            var text = GetComponent<TextMeshPro>();

            if (text.color != modulate * baseColor)
                text.color = modulate * baseColor;
        }
    }
}
