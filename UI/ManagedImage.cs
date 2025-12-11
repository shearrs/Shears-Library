using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Image))]
    public class ManagedImage : MonoBehaviour, IColorTweenable
    {
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color modulate = Color.white;

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

        public Color BaseColor
        {
            get => baseColor;
            set 
            {
                baseColor = value;

                RawImage.color = baseColor * modulate;
            }
        }

        public Color Modulate
        {
            get => modulate;
            set
            {
                modulate = value;

                var color = baseColor * modulate;
                color.a = baseColor.a * modulate.a;
                RawImage.color = color;
            }
        }
    
        public Sprite Sprite { get => RawImage.sprite; set => RawImage.sprite = value; }
    }
}
