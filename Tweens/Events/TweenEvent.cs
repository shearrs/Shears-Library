using System;
using UnityEngine;

namespace Shears.Tweens
{
    public class TweenEvent : TweenEventBase
    {
        private readonly float progress;

        public event Action ProgressReached;

        public TweenEvent(float progress)
        {
            this.progress = progress;
        }

        public override bool CanInvoke(float t)
        {
            return t >= progress;
        }

        public override void Invoke()
        {
            ProgressReached?.Invoke();
        }
    }
}
