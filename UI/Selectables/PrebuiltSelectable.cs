using Shears.Common;
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
        [SerializeField] private ColorPaletteHandle defaultColor;
        [SerializeField] private ColorPaletteHandle hoverColor;

        private ITween tween;

        protected override void OnEnable()
        {
            base.OnEnable();

            targetGraphic.color = defaultColor.GetColor();
        }

        internal override void Select()
        {
            PlayTween(defaultColor.GetColor(), hoverColor.GetColor());
            base.Select();
        }

        internal override void Unselect()
        {
            if (gameObject.activeSelf)
                PlayTween(hoverColor.GetColor(), defaultColor.GetColor());

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
