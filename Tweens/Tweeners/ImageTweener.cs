using UnityEngine;
using UnityEngine.UI;

namespace Shears.Tweens
{
    public class ImageTweener : MonoBehaviour
    {
        private enum TweenType { Color }

        [Header("Data")]
        [SerializeField] private TweenData data;
        [SerializeField] private TweenType type;
        [SerializeField] private bool playOnEnable;
        [SerializeField] private Image image;
        private ITween tween;

        [Header("Input")]
        [SerializeField] private Color color = Color.blue;

        [Header("Initialization")]
        [SerializeField] private bool initializeOnEnable;
        [SerializeField] private Color initialColor = Color.white;

        private void OnEnable()
        {
            if (initializeOnEnable)
                SetInitialValues();

            if (playOnEnable)
                Play();
        }

        public void Play()
        {
            ClearTween();

            SetInitialValues();
            tween = GetTween();
            tween.Play();
        }

        public void ClearTween()
        {
            tween?.Stop();
            tween?.Dispose();

            tween = null;
        }

        private ITween GetTween()
        {
            ITween tween = null;

            switch(type)
            {
                case TweenType.Color: 
                    tween = image.GetColorTween(color, data);
                    break;
            }

            tween.AddOnComplete(ClearTween);

            return tween;
        }

        public void SetInitialValues()
        {
            image.color = initialColor;
        }
    }
}
