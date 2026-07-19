using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Image))]
    public class UIImage : UIElement
    {
        [SerializeField]
        private Color baseColor = Color.white;

        [SerializeField]
        private Color modulate = Color.white;

        [SerializeField]
        private bool additiveModulate = false;

        private Image image;

        public Image RawImage
        {
            get
            {
                if (image == null)
                    image = GetComponent<Image>();

                return image;
            }
        }

        public Sprite Sprite
        {
            get => RawImage.sprite;
            set => RawImage.sprite = value;
        }

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

        private void Reset()
        {
            BaseColor = RawImage.color;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Application.isPlaying)
                return;

            var image = GetComponent<Image>();
            var targetColor = AdditiveModulate
                ? (modulate + baseColor).With(a: Alpha)
                : (modulate * baseColor).With(a: Alpha);

            if (image.color != targetColor)
                image.color = targetColor;
        }

        protected override void Repaint(StyleData data)
        {
            RawImage.color = data.Color;
        }
    }
}
