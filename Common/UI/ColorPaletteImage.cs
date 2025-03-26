using Shears.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Shears
{
    public class ColorPaletteImage : MonoBehaviour
    {
        [SerializeField] private ColorPaletteHandle paletteHandle;
        private Image image;

        private void OnValidate()
        {
            if (!paletteHandle.IsValidColor())
                return;

            if (TryGetComponent(out image))
                image.color = paletteHandle.GetColor();
            else
                Debug.LogError("No image attached to ColorPaletteImage", this);
        }
    }
}
