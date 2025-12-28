using Shears.Tweens;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.Loading
{
    public class DefaultLoadingScreen : LoadingScreen
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform container;
        [SerializeField] private Slider loadingBar;

        private CoroutineChain coroutineChain;

        public Canvas Canvas => canvas;

        public event Action Enabled;
        public event Action PreDisabled;
        public event Action Disabled;

        private void Awake()
        {
            coroutineChain = CoroutineChain.Create().WithLifetime(this);
        }

        public override Coroutine Enable()
        {
            return StartCoroutine(IEFadeIn());
        }

        public override Coroutine Disable()
        {
            return StartCoroutine(IEFadeOut());
        }

        private IEnumerator IEFadeIn()
        {
            Enabled?.Invoke();

            canvas.enabled = true;
            IsDelaying = true;

            var color = backgroundImage.color.With(a: 1.0f);
            var transparentColor = color.With(a: 0.0f);

            backgroundImage.color = transparentColor;

            var tween = backgroundImage.DoColorTween(color);

            while (tween.IsPlaying)
                yield return null;

            container.gameObject.SetActive(true);

            coroutineChain
                .Tween((t) =>
                {
                    loadingBar.value = t;
                }, 3.0f)
                .Then(() => IsDelaying = false);

            yield return coroutineChain.Start();

            StartCoroutine(IEDelay());
        }

        private IEnumerator IEDelay()
        {
            yield return CoroutineUtil.WaitForSeconds(2.0f);

            IsDelaying = false;
        }

        private IEnumerator IEFadeOut()
        {
            PreDisabled?.Invoke();
            container.gameObject.SetActive(false);

            var transparentColor = backgroundImage.color.With(a:0.0f);
            var tween = backgroundImage.DoColorTween(transparentColor);

            while (tween.IsPlaying)
                yield return null;

            canvas.enabled = false;

            Disabled?.Invoke();
        }
    }
}
