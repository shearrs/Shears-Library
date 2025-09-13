using UnityEngine;

namespace Shears.Tweens
{
    public interface IColorTweenable
    {
        public Color BaseColor { get; set; }
        public Color Modulate { get; set; }
    }
}
