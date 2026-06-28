using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    public class UIElementTests : MonoBehaviour
    {
        private enum TestType
        {
            FadeOut,
            FadeIn,
            FadeAllOut,
            FadeAllIn,
            Color,
            ColorAndFade,
        }

        [Header("Element")]
        [SerializeField]
        private UIElement element;

        [SerializeField]
        private bool run;

        [Header("Test Settings")]
        [SerializeField]
        private TestType type;

        [SerializeField]
        private TweenData tweenData;

        [SerializeField]
        private Color color = Color.white;

        private void Update()
        {
            if (run)
            {
                run = false;
                var children = element.GetComponentsInChildren<UIElement>();

                switch (type)
                {
                    case TestType.FadeOut:
                        element.DoFadeTween(0.0f, tweenData);
                        break;
                    case TestType.FadeIn:
                        element.DoFadeTween(1.0f, tweenData);
                        break;
                    case TestType.FadeAllOut:
                        foreach (var child in children)
                            child.DoFadeTween(0.0f, tweenData);
                        break;
                    case TestType.FadeAllIn:
                        foreach (var child in children)
                            child.DoFadeTween(1.0f, tweenData);
                        break;
                    case TestType.Color:
                        foreach (var child in children)
                            child.DoModulateTween(color, tweenData);
                        break;
                    case TestType.ColorAndFade:
                        foreach (var child in children)
                        {
                            child.DoModulateTween(color, tweenData);
                            child.DoFadeTween(0.0f, tweenData);
                        }
                        break;
                }
            }
        }
    }
}
