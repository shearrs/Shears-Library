using System.Collections.Generic;
using UnityEngine;

namespace Shears.Common
{
    [CreateAssetMenu(fileName = "New Color Palette Set", menuName = "UI/Color Palette Set")]
    public class ColorPaletteSet : ScriptableObject
    {
        [SerializeField] private List<ColorPalette> colorPalettes = new();

        public IReadOnlyList<ColorPalette> Palettes => colorPalettes;

        public Color GetColor(int paletteIndex, int colorIndex)
        {
            if (paletteIndex >= colorPalettes.Count)
            {
                Debug.LogError($"Palette index {paletteIndex} is out of range", this);
                return Color.magenta;
            }
            else if (colorIndex >= colorPalettes[paletteIndex].Colors.Count)
            {
                Debug.LogError($"Color index {colorIndex} is out of range", this);
                return Color.magenta;
            }

            return colorPalettes[paletteIndex].Colors[colorIndex];
        }
    }
}