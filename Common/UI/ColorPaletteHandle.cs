using UnityEngine;

namespace Shears.Common
{
    [System.Serializable]
    public class ColorPaletteHandle
    {
        [SerializeField] private ColorPalette palette;
        [SerializeField] private int colorIndex;

        public Color Color => palette.Colors[colorIndex];
    }
}
