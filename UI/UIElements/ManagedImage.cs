using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class ManagedImage : MonoBehaviour
    {
        [SerializeField] private Image image;

        public Sprite Sprite { get => image.sprite; set => image.sprite = value; }

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
