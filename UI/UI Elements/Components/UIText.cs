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

        [SerializeField]
        private bool additiveModulate;

        private TextMeshPro textMesh;

        protected override Color BaseColorValue
        {
            get => baseColor;
            set => baseColor = value;
        }
        protected override Color ModulateValue
        {
            get => modulate;
            set => modulate = value;
        }
        protected override bool AdditiveModulateValue
        {
            get => additiveModulate;
            set => additiveModulate = value;
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
            if (Application.isPlaying)
                return;

            var text = GetComponent<TextMeshPro>();
            var targetColor = AdditiveModulate
                ? (modulate + baseColor).With(a: Alpha)
                : (modulate * baseColor).With(a: Alpha);

            if (text.color != targetColor)
                text.color = targetColor;
        }

        protected override void Repaint(StyleData data)
        {
            TextMesh.color = data.Color;
        }
    }
}
