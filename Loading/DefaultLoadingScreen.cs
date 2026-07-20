using System;
using System.Collections;
using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.Loading
{
    public class DefaultLoadingScreen : LoadingScreen
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private RectTransform container;

        [SerializeField]
        private Slider loadingBar;

        private readonly TweenData tweenData = new(unscaledTime: true);

        public Canvas Canvas => canvas;

        public event Action Enabled;
        public event Action PreDisabled;
        public event Action Disabled;

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

            backgroundImage.color = backgroundImage.color.With(a: 0.0f);

            var tween = backgroundImage.DoFadeTween(1.0f, tweenData);

            while (tween.IsPlaying)
                yield return null;

            container.gameObject.SetActive(true);

            float elapsedTime = 0.0f;

            while (elapsedTime < 3.0f)
            {
                loadingBar.value = Mathf.Lerp(0, 1, elapsedTime / 3.0f);

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            StartCoroutine(IEDelay());
        }

        private IEnumerator IEDelay()
        {
            yield return CoroutineUtil.WaitForSecondsRealtime(1.0f);

            IsDelaying = false;
        }

        private IEnumerator IEFadeOut()
        {
            PreDisabled?.Invoke();
            container.gameObject.SetActive(false);

            var tween = backgroundImage.DoFadeTween(0.0f, tweenData);

            while (tween.IsPlaying)
                yield return null;

            canvas.enabled = false;

            Disabled?.Invoke();
        }
    }
}
