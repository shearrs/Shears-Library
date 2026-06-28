using TMPro;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextGUI : UIElement
    {
        [SerializeField]
        private Color baseColor = Color.white;

        [SerializeField]
        private Color modulate = Color.white;

        private TextMeshProUGUI textMesh;

        public TextMeshProUGUI TextMesh
        {
            get
            {
                if (textMesh == null)
                    textMesh = GetComponent<TextMeshProUGUI>();

                return textMesh;
            }
        }
        public override Color BaseColor
        {
            get => baseColor;
            set
            {
                baseColor = value;
                TextMesh.color = Modulate * baseColor;
            }
        }
        public override Color Modulate
        {
            get => modulate;
            set
            {
                modulate = value;
                TextMesh.color = Modulate * baseColor;
            }
        }
        public string Text
        {
            get => TextMesh.text;
            set => TextMesh.text = value;
        }

        private void OnValidate()
        {
            var text = GetComponent<TextMeshProUGUI>();

            if (text.color != modulate * baseColor)
                text.color = modulate * baseColor;
        }
    }
}
