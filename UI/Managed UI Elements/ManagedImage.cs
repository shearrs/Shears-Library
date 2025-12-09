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

        protected Image Image
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

                Image.color = baseColor * modulate;
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
                Image.color = color;
            }
        }
    }
}
