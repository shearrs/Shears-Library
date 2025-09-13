using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [RequireComponent(typeof(Image))]
    public class ManagedImage : MonoBehaviour, IColorTweenable
    {
        [SerializeField] private Color modulate = Color.white;
        private Image image;

        private bool initialized = false;
        private Color baseColor;

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
            get
            {
                if (!initialized)
                    baseColor = Image.color;

                return baseColor;
            }
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

                Image.color = baseColor * modulate;
            }
        }

        private void Awake()
        {
            baseColor = Image.color;

            initialized = true;
        }
    }
}
