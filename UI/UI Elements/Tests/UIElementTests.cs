using Shears.Tweens;
using UnityEngine;

namespace Shears.UI
{
    public class UIElementTests : MonoBehaviour
    {
        private enum TestType
        {
            FadeIn,
            FadeOut,
            Color,
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

        [SerializeField, ShowIf(nameof(type), TestType.Color)]
        private Color color;

        private void Update()
        {
            if (run)
            {
                run = false;

                switch (type)
                {
                    case TestType.FadeIn:
                        element.DoFadeTween(1.0f, tweenData);
                        break;
                    case TestType.FadeOut:
                        element.DoFadeTween(0.0f, tweenData);
                        break;
                    case TestType.Color:
                        element.DoColorTween(color, tweenData);
                        break;
                }
            }
        }
    }
}
