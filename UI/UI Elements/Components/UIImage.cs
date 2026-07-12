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

        private void Reset()
        {
            BaseColor = RawImage.color;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            RawImage.color = modulate * baseColor;
        }

        protected override void ApplyResolvedStyle(StyleData data)
        {
            RawImage.color = data.Color;
        }
    }
}
