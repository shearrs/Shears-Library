using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears.Tweens
{
    public class TweenManager : PersistentProtectedSingleton<TweenManager>
    {
        [SerializeField] private List<TweenInstance> tweens = new();
        private ObjectPool<TweenInstance> tweenPool;
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
            TweenInstance tween = tweenPool.Get();
            
            tween.Update = update;
            tween.Release = Release;
            tween.IsActive = true;

            data ??= defaultTweenData;

            tween.SetData(data);
            tween.AddDisposeEvent(GetApplicationDisposeEvent());

            return new(tween);
        }

        private TweenStopEvent GetApplicationDisposeEvent()
        {
            static bool disposeEvent() => !Application.isPlaying;

            return disposeEvent;
        }
        #endregion

        private static Tween Do(Tween tween)
        {
            tween.Play();

            return tween;
        }

        private void Release(TweenInstance tween)
        {
            tween.IsActive = false;

            tweenPool.Release(tween);
        }

        #region Pool
        private TweenInstance PoolCreate()
        {
            TweenInstance tween = new();

            tweens.Add(tween);

            return tween;
        }

        private void PoolGet(TweenInstance tween)
        {
            ResetTween(tween);
        }

        private void ResetTween(TweenInstance tween)
        {
            tween.ClearOnCompletes();
            tween.ClearStopEvents();
            tween.ClearDisposeEvents();
            tween.ClearEvents();
        }
        #endregion
    }
}
