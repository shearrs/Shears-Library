using UnityEngine;
using UnityEngine.UI;

namespace Shears.Tweens
{
    public class ImageTweener : MonoBehaviour
    {
        public enum TweenType { Color }

        [Header("Data")]
        [SerializeField] private bool playOnEnable;
        [SerializeField] private Image image;
        [SerializeField] private TweenDataObject data;
        [SerializeField] private TweenType type;
        private Tween tween;

        [Header("Colors")]
        [SerializeField] private Color color1 = Color.white;
        [SerializeField] private Color color2 = Color.gray;

        public TweenDataObject TweenData { get => data; set => data = value; }
        public Image Image { get => image; set => image = value; }
        public TweenType Type { get => type; set => type = value; }
        public Color Color1 { get => color1; set => color1 = value; }
        public Color Color2 { get => color2; set => color2 = value; }

        private void OnEnable()
        {
            if (playOnEnable)
                Play1To2();
        }

        public void Play1To2() => Play(GetTween(color1, color2));
        public void Play2To1() => Play(GetTween(color2, color1));

        private void Play(Tween tween)
        {
            ClearTween();

            this.tween = tween;
            tween.Play();
        }

        public void Stop() => ClearTween();

        private void ClearTween()
        {
            tween?.Stop();
            tween?.Dispose();

            tween = null;
        }

        private Tween GetTween(Color from, Color to)
        {
            Tween tween = null;

            image.color = from;

            switch(type)
            {
                case TweenType.Color: 
                    tween = image.GetColorTween(to, data);
                    break;
            }

            tween.AddOnComplete(ClearTween);

            return tween;
        }
    }
}
