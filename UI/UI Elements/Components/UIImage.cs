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

        private void Reset()
        {
            BaseColor = RawImage.color;
        }

        protected override Color GetBaseColor()
        {
            return baseColor;
        }

        protected override void SetBaseColor(Color color)
        {
            baseColor = color;

            RawImage.color = baseColor * modulate;
        }

        protected override Color GetModulate()
        {
            return modulate;
        }

        protected override void SetModulate(Color color)
        {
            modulate = color;

            var finalColor = baseColor * modulate;
            finalColor.a = baseColor.a * modulate.a;
            RawImage.color = finalColor;
        }
    }
}
