using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    public interface ITweenData
    {
        public float Duration { get; }
        public bool ForceFinalValue { get; }
        public int Loops { get; }
        public LoopMode LoopMode { get; }
        public Ease EasingFunction { get; }
        public bool UsesCurve { get; }
        public AnimationCurve Curve { get; }
    }
}
