using System;
using UnityEngine;

namespace Shears.Common
{
    [Serializable]
    public class ColorPaletteHandle : ISerializationCallbackReceiver
    {
        [SerializeField] private ColorPaletteSet paletteSet;
        [SerializeField, Min(0)] private int paletteIndex = 0;
        [SerializeField, Min(0)] private int colorIndex = 0;
        [SerializeField, Range(0, 1)] private float alpha = 1;
        private ColorPaletteSet previousSet;

        public ColorPaletteSet PaletteSet => paletteSet;

        public event Action OnPaletteChanged;

        public Color GetColor()
        {
            if (paletteSet == null)
                return ColorPalette.NULL_COLOR;

            Color color = paletteSet.GetColor(paletteIndex, colorIndex);
            color.a = alpha;

            return color;
        }

        #region Setters
        public void SetPaletteIndex(int value)
        {
            SetPaletteIndexInternal(value);

            OnPaletteChanged?.Invoke();
        }

        public void SetColorIndex(int value)
        {
            SetColorIndexInternal(value);

            OnPaletteChanged?.Invoke();
        }

        private void SetPaletteIndexInternal(int value)
        {
            if (!IsPaletteSetValid())
                return;

            paletteIndex = Mathf.Clamp(value, 0, paletteSet.Palettes.Count - 1);
        }

        private void SetColorIndexInternal(int value)
        {
            if (!IsPaletteSetValid())
                return;

            var palette = paletteSet.Palettes[paletteIndex];

            colorIndex = Mathf.Clamp(value, 0, palette.Colors.Count - 1);
        }
        #endregion

        #region Serialization
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            RefreshPaletteEvents();

            if (!IsPaletteSetValid())
                return;

            SetPaletteIndexInternal(paletteIndex);
            SetColorIndexInternal(colorIndex);
        }
        #endregion

        private bool IsPaletteSetValid() => paletteSet != null && paletteSet.Palettes.Count > 0;

        private void OnSetChanged()
        {
            OnPaletteChanged?.Invoke();
        }

        private void RefreshPaletteEvents()
        {
            if (previousSet != null && previousSet != paletteSet)
            {
                previousSet.OnSetChanged -= OnSetChanged;
                previousSet = paletteSet;
            }

            if (paletteSet != null)
            {
                paletteSet.OnSetChanged -= OnSetChanged;
                paletteSet.OnSetChanged += OnSetChanged;
            }
        }
    }
}
