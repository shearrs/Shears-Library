using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class ManagedImage : MonoBehaviour, IColorTweenable
    {
        [SerializeField] private Image image;
        [SerializeField] private Color baseColor;

        public Color BaseColor { get => baseColor; set => baseColor = value; }
        public Color CurrentColor { get => image.color; set => image.color = value; }

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
