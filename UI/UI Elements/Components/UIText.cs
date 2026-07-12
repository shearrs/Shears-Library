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

        public override Color BaseColor
        {
            get => baseColor;
            set => baseColor = value;
        }
        public override Color Modulate
        {
            get => modulate;
            set => modulate = value;
        }
        public TextMeshPro TextMesh
        {
            get
            {
                if (textMesh == null)
                    textMesh = GetComponent<TextMeshPro>();

                return textMesh;
            }
        }

        public string Text
        {
            get => TextMesh.text;
            set => TextMesh.text = value;
        }

        private void Reset()
        {
            var text = GetComponent<TextMeshPro>();
            baseColor = text.color;
        }

        private void OnValidate()
        {
            var text = GetComponent<TextMeshPro>();
            var targetColor = (modulate * baseColor).With(a: Alpha);

            if (text.color != targetColor)
                text.color = targetColor;
        }

        protected override void ApplyResolvedStyle(StyleData data)
        {
            TextMesh.color = data.Color;
        }
    }
}
