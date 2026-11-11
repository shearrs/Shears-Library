using UnityEngine;

namespace Shears.Signals
{
    public readonly struct ShakeCameraSignal : ISignal
    {
        public float Strength { get; }
        public float ShakeDelay { get; }
        public float Duration { get; }

        public ShakeCameraSignal(float strength, float shakeDelay = 0.0f, float duration = 0.5f)
        {
            Strength = strength;
            ShakeDelay = shakeDelay;
            Duration = duration;
        }
    }
}
