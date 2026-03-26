using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    public class TweenStorage
    {
        private readonly List<Tween> tweens = new();

        public IReadOnlyList<Tween> Tweens => tweens;

        public bool HasValidTween()
        {
            return GetFirstValid() != Tween.Empty;
        }

        public Tween GetFirstValid()
        {
            foreach (var tween in tweens)
            {
                if (tween.IsValid)
                    return tween;
            }

            return Tween.Empty;
        }

        public Tween Store(Tween tween)
        {
            tweens.Add(tween);

            tween.Completed += () => tweens.Remove(tween);

            return tween;
        }

        public void Dispose()
        {
            foreach (var tween in tweens)
                tween.Dispose();

            tweens.Clear();
        }
        
        public Tween this[int index] => tweens[index];
    }
}
