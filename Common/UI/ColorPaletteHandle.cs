using UnityEngine;

namespace Shears.Common
{
    [System.Serializable]
    public class ColorPaletteHandle : ISerializationCallbackReceiver
    {
        [SerializeField] private ColorPaletteSet paletteSet;
        [SerializeField] private int paletteIndex = 0;
        [SerializeField] private int colorIndex = 0;

        public bool IsValidColor()
        {
            return paletteSet != null && paletteSet.Palettes.Count > paletteIndex && paletteSet.Palettes[paletteIndex].Colors.Count > colorIndex;
        }

        public Color GetColor() => paletteSet.GetColor(paletteIndex, colorIndex);

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (paletteSet == null)
                return;

            paletteIndex = Mathf.Clamp(paletteIndex, 0, paletteSet.Palettes.Count - 1);

            if (paletteIndex == -1)
            {
                paletteIndex = 0;
                return;
            }    

            var palette = paletteSet.Palettes[paletteIndex];

            colorIndex = Mathf.Clamp(colorIndex, 0, palette.Colors.Count - 1);

            if (colorIndex == -1)
                colorIndex = 0;
        }
    }
}
