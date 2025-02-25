using System.Collections.Generic;
using UnityEngine;

namespace Shears.Common
{
    [CreateAssetMenu(fileName = "New Color Palette", menuName = "Color Palette")]
    public class ColorPalette : ScriptableObject
    {
        [SerializeField] private List<Color> colors;

        public IReadOnlyList<Color> Colors => colors;
    }
}