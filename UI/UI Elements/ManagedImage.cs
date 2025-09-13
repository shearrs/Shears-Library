using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class ManagedImage : MonoBehaviour, IColorTweenable
    {
        [SerializeField] private Image image;
        [SerializeField] private Color modulate = Color.white;

        private bool initialized = false;
        private Color baseColor;

        public Color BaseColor
        {
            get
            {
                if (!initialized)
                    baseColor = image.color;

                return baseColor;
            }
            set 
            { 
                baseColor = value;

                image.color = baseColor * modulate;
            }
        }
        public Color Modulate
        {
            get => modulate;
            set
            {
                modulate = value;

                image.color = baseColor * modulate;
            }
        }

        private void Awake()
        {
            baseColor = image.color;

            initialized = true;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
