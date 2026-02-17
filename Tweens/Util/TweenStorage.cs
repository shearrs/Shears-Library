using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    public class TweenStorage
    {
        private readonly List<Tween> tweens = new();

        public IReadOnlyList<Tween> Tweens => tweens;

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
    }
}
