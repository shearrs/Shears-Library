using UnityEngine;

namespace Shears.Signals
{
    public readonly struct ShakeCameraSignal : ISignal
    {
        public float Strength { get; }
        public float ShakeDelay { get; }
        public float Duration { get; }

        public ShakeCameraSignal(float strength, float shakeDelay = 0.01f, float duration = 0.2f)
        {
            Strength = strength;
            ShakeDelay = shakeDelay;
            Duration = duration;
        }
    }
}
