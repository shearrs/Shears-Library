using System.Collections.Generic;
using UnityEngine;

namespace Shears.Common
{
    [System.Serializable]
    public class ColorPalette
    {
        [SerializeField] private List<Color> colors;

        public IReadOnlyList<Color> Colors => colors;

        public static readonly Color NULL_COLOR = Color.magenta;
    }
}
