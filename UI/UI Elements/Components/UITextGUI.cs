using Shears.Tweens;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
        public string Text
        {
            get => TextMesh.text;
            set => TextMesh.text = value;
        }

        private void Reset()
        {
            var text = GetComponent<TextMeshProUGUI>();

            baseColor = text.color;
        }

        private void OnValidate()
        {
            var text = GetComponent<TextMeshProUGUI>();

            if (text.color != modulate * baseColor)
                text.color = modulate * baseColor;
        }

        public Tween DoCounterTween(
            int targetNumber,
            int startNumber = 0,
            string prefix = "",
            string suffix = "",
            ITweenData data = null
        )
        {
            return textMesh.DoCounterTween(targetNumber, startNumber, prefix, suffix, data);
        }

        public Tween GetCounterTween(
            int targetNumber,
            int startNumber = 0,
            string prefix = "",
            string suffix = "",
            ITweenData data = null
        )
        {
            return textMesh.GetCounterTween(targetNumber, startNumber, prefix, suffix, data);
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
