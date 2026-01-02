using System;
using UnityEngine;

namespace Shears.Tweens
{
    internal readonly struct TweenUpdate
    {
        private readonly Action<float> floatUpdate;
        
        public TweenUpdate(Action<float> floatUpdate)
        {
            this.floatUpdate = floatUpdate;
        }

        public void Invoke(float t)
        {
            floatUpdate?.Invoke(t);
        }
    }
}
