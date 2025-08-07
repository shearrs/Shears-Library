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
        private TweenData defaultTweenData;

        protected override void Awake()
        {
            base.Awake();

            tweenPool = new(PoolCreate, PoolGet);

            defaultTweenData = Resources.Load<TweenData>("Tween Data/Default Tween Data");
        }

        #region Custom Tween
        public static Tween DoTween(Action<float> update, TweenData data) => Do(CreateTween(update, data));
        public static Tween CreateTween(Action<float> update, TweenData data) => Instance.InstCreateTween(update, data);
        private Tween InstCreateTween(Action<float> update, TweenData data)
        {
            Tween tween = tweenPool.Get();

            tween.Update = update;
            tween.Release = Release;
            tween.IsActive = true;

            if (data == null)
                data = defaultTweenData;

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
        }
        #endregion
    }
}
