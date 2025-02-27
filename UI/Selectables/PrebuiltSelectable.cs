using Shears.Tweens;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class PrebuiltSelectable : ManagedSelectable
    {
        [Header("Graphics")]
        [SerializeField] private Image targetGraphic;
        [SerializeField] private TweenData hoverTweenData;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new(1, 1, 1, 0);
        [SerializeField] private Color hoverColor = Color.white;

        private ITween tween;

        protected override void OnEnable()
        {
            base.OnEnable();

            targetGraphic.color = defaultColor;
        }

        internal override void Select()
        {
            PlayTween(defaultColor, hoverColor);
            base.Select();
        }

        internal override void Unselect()
        {
            if (gameObject.activeSelf)
                PlayTween(hoverColor, defaultColor);

            base.Unselect();
        }

        private void PlayTween(Color start, Color end)
        {
            ClearTween();

            targetGraphic.color = start;
            tween = GetTween(end);

            tween.Play();
        }
        
        private ITween GetTween(Color targetColor)
        {
            var tween = targetGraphic.GetColorTween(targetColor, hoverTweenData);

            tween.AddOnComplete(ClearTween);

            return tween;
        }

        private void ClearTween()
        {
            tween?.Dispose();

            tween = null;
        }
    }
}
