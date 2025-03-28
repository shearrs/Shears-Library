using Shears.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [ExecuteInEditMode]
    public class ColorPaletteImage : MonoBehaviour
    {
        [SerializeField] private ColorPaletteHandle paletteHandle = new();
        private Image image;

        public ColorPaletteHandle PaletteHandle => paletteHandle;

        private void OnValidate()
        {
            UpdateColor();
        }

        private void OnEnable()
        {
            paletteHandle.OnPaletteChanged += UpdateColor;
        }

        private void OnDisable()
        {
            paletteHandle.OnPaletteChanged -= UpdateColor;
        }

        private void UpdateColor()
        {
            if (image == null)
            {
                if (!TryGetComponent(out image))
                    return;
            }

            image.color = paletteHandle.GetColor();
        }
    }
}
