using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears.Tweens
{
    public class TweenManager : PersistentProtectedSingleton<TweenManager>
    {
        [SerializeField] private List<Tween> tweens = new();
        private ObjectPool<Tween> tweenPool;
        private ITweenData defaultTweenData;

        protected override void Awake()
        {
            base.Awake();

            tweenPool = new(PoolCreate, PoolGet);

            defaultTweenData = Resources.Load<TweenDataObject>("Tween Data/Default Tween Data");
        }

        #region Custom Tween
        public static Tween DoTween(Action<float> update, ITweenData data = null) => Do(CreateTween(update, data));
        public static Tween CreateTween(Action<float> update, ITweenData data = null) => Instance.InstCreateTween(update, data);
        private Tween InstCreateTween(Action<float> update, ITweenData data)
        {
            Tween tween = tweenPool.Get();

            tween.Update = update;
            tween.Release = Release;
            tween.IsActive = true;

            data ??= defaultTweenData;

            tween.SetData(data);

            return tween;
        }
        #endregion

        private static Tween Do(Tween tween)
        {
            tween.Play();

            return tween;
        }

        private void Release(Tween tween)
        {
            tween.IsActive = false;

            tweenPool.Release(tween);
        }

        #region Pool
        private Tween PoolCreate()
        {
            Tween tween = new();

            tweens.Add(tween);

            return tween;
        }

        private void PoolGet(Tween tween)
        {
            ResetTween(tween);
        }

        private void ResetTween(Tween tween)
        {
            tween.ClearOnCompletes();
            tween.ClearStopEvents();
            tween.ClearDisposeEvents();
            tween.ClearEvents();
        }
        #endregion
    }
}
