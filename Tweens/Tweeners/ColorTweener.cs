using UnityEngine;

namespace Shears.Tweens
{
    public class ColorTweener : MonoBehaviour
    {
        public enum OnEnableBehaviour { None, Play1To2, Play2To1 };
        public enum TweenType { Override, Multiply }

        [Header("Settings")]
        [SerializeField] private OnEnableBehaviour onEnableBehaviour;
        [SerializeField] private TweenType type;

        [Header("References")]
        [SerializeField] private InterfaceReference<IColorTweenable> target;
        [SerializeField] private TweenData tweenData;
        private Tween tween;

        [Header("Colors")]
        [SerializeField] private Color color1 = Color.white;
        [SerializeField] private Color color2 = Color.gray;

        public TweenData TweenData { get => tweenData; set => tweenData = value; }
        public IColorTweenable Target { get => target.Value; set => target.Value = value; }
        public Color Color1 { get => color1; set => color1 = value; }
        public Color Color2 { get => color2; set => color2 = value; }

        private void OnEnable()
        {
            switch (onEnableBehaviour)
            {
                case OnEnableBehaviour.None:
                    break;
                case OnEnableBehaviour.Play1To2:
                    Play1To2();
                    break;
                case OnEnableBehaviour.Play2To1:
                    Play2To1();
                    break;
            }
        }

        private void OnDisable() => Stop();

        public void SetColor1()
        {
            Color color = color1;

            if (type == TweenType.Multiply)
                color = Target.BaseColor * color1;

            Target.Modulate = color;
        }
        public void SetColor2()
        {
            Color color = color1;

            if (type == TweenType.Multiply)
                color = Target.BaseColor * color2;

            Target.Modulate = color;
        }

        public void Play1To2()
        {
            Color initial = Target.BaseColor;

            if (type == TweenType.Multiply)
                initial = Target.BaseColor * Color1;

            Play(GetTween(initial, color1, color2));
        }
        public void Play2To1()
        {
            Color initial = Color2;

            if (type == TweenType.Multiply)
                initial = Target.BaseColor * Color2;

            Play(GetTween(initial, color2, color1));
        }

        private void Play(Tween tween)
        {
            ClearTween();

            this.tween = tween;
            tween.Play();
        }

        public void Stop() => ClearTween();

        private void ClearTween()
        {
            tween.Dispose();
        }

        private Tween GetTween(Color initial, Color from, Color to)
        {
            Target.Modulate = initial;

            var tween = type switch
            {
                TweenType.Override => Target.GetColorTween(to, tweenData),
                TweenType.Multiply => Target.GetColorMultTween(Target.BaseColor, from, to, tweenData),
                _ => Target.GetColorTween(to, tweenData),
            };

            tween.AddOnComplete(ClearTween);

            return tween;
        }
    }
}
