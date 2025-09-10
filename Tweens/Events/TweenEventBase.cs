using System;
using UnityEngine;

namespace Shears.Tweens
{
    /// <summary>
    /// Base class for TweenEvents. 
    /// </summary>
    public abstract class TweenEventBase
    {
        public abstract bool CanInvoke(float t);
        public abstract void Invoke();
    }
}
