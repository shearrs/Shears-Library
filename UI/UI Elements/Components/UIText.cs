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

            if (text.color != modulate * baseColor)
                text.color = modulate * baseColor;
        }

        protected override Color GetBaseColor()
        {
            return baseColor;
        }

        protected override void SetBaseColor(Color color)
        {
            baseColor = color;
            TextMesh.color = Modulate * baseColor;
        }

        protected override Color GetModulate()
        {
            return modulate;
        }

        protected override void SetModulate(Color color)
        {
            modulate = color;
            TextMesh.color = Modulate * baseColor;
        }
    }
}
