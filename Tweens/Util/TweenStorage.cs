using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.Tweens
{
    public class TweenStorage
    {
        private readonly List<Tween> tweens = new();
        private readonly HashSet<TweenStorage> subStorage = new();

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

            foreach (var storage in subStorage)
            {
                var tween = storage.GetFirstValid();

                if (tween.IsValid)
                    return tween;
            }

            return Tween.Empty;
        }

        public Tween Store(Tween tween)
        {
            if (tween == Tween.Empty)
                return tween;

            tweens.Add(tween);

            tween.Completed += () => tweens.Remove(tween);

            return tween;
        }

        public void Dispose()
        {
            foreach (var tween in tweens)
                tween.Dispose();

            foreach (var storage in subStorage)
                storage.Dispose();

            tweens.Clear();
        }

        public void AddSubStorage(TweenStorage storage)
        {
            if (subStorage.Contains(storage))
                return;

            subStorage.Add(storage);
        }

        public void RemoveSubStorage(TweenStorage storage)
        {
            subStorage.Remove(storage);
        }

        public Tween this[int index] => tweens[index];
    }
}
