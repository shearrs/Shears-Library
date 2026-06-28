using TMPro;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class UIText : UIElement
    {
        private TextMeshPro textMesh;
        private Color baseColor;
        private Color modulate;

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
    }
}
