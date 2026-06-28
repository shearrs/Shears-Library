using UnityEngine;

namespace Shears.Tweens
{
    public interface IColorTweenable : IAlphaTweenable
    {
        public Color BaseColor { get; set; }
        public Color Modulate { get; set; }
        float IAlphaTweenable.Alpha
        {
            get => Modulate.a;
            set => Modulate = Modulate.With(a: value);
        }
    }
}
