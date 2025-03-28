using Shears.Common;
using TMPro;
using UnityEngine;

namespace Shears.UI
{
    [ExecuteInEditMode]
    public class ColorPaletteText : MonoBehaviour
    {
        [SerializeField] private ColorPaletteHandle paletteHandle = new();
        private TextMeshProUGUI textMesh;

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
            if (textMesh == null)
            {
                if (!TryGetComponent(out textMesh))
                    return;
            }

            textMesh.color = paletteHandle.GetColor();
        }
    }
}
