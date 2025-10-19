using Shears.Tweens;
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
            canvas.enabled = true;
            IsDelaying = true;

            var color = backgroundImage.color;
            var transparentColor = color;
            transparentColor.a = 0;

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
            canvas.enabled = false;
            container.gameObject.SetActive(false);

            yield break;
        }
    }
}
