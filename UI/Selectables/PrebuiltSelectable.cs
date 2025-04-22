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
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color hoverColor = Color.white;
        [SerializeField] private Color selectColor = Color.white;

        private ITween tween;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (targetGraphic != null)
                targetGraphic.color = defaultColor;
        }

        internal override void Hover()
        {
            PlayTween(defaultColor, hoverColor);
            base.Hover();
        }

        internal override void Unhover()
        {
            PlayTween(hoverColor, defaultColor);
            base.Unhover();
        }

        internal override void Select()
        {
            PlayTween(defaultColor, selectColor);
            base.Select();
        }

        internal override void Unselect()
        {
            if (gameObject.activeSelf)
                PlayTween(selectColor, defaultColor);

            base.Unselect();
        }

        private void PlayTween(Color start, Color end)
        {
            if (targetGraphic == null)
                return;

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
