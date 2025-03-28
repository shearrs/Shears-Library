using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Common
{
    [CreateAssetMenu(fileName = "New Color Palette Set", menuName = "UI/Color Palette Set")]
    public class ColorPaletteSet : ScriptableObject
    {
        [SerializeField] private List<ColorPalette> colorPalettes = new();

        public IReadOnlyList<ColorPalette> Palettes => colorPalettes;

        public event Action OnSetChanged;

        private void OnValidate()
        {
            OnSetChanged?.Invoke();
        }

        private void OnDestroy()
        {
            OnSetChanged = null;
        }

        public Color GetColor(int paletteIndex, int colorIndex)
        {
            if (colorPalettes.Count == 0)
                return ColorPalette.NULL_COLOR;

            if (paletteIndex >= colorPalettes.Count)
            {
                Debug.LogError($"Palette index {paletteIndex} is out of range", this);
                return ColorPalette.NULL_COLOR;
            }
            else if (colorIndex >= colorPalettes[paletteIndex].Colors.Count)
            {
                Debug.LogError($"Color index {colorIndex} is out of range", this);
                return ColorPalette.NULL_COLOR;
            }

            return colorPalettes[paletteIndex].Colors[colorIndex];
        }
    }
}