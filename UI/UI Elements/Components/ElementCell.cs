using Shears.Tweens;
using System;
using UnityEngine;

namespace Shears.UI
{
    /// <summary>
    /// Holds a <see cref="CellContent"/>.
    /// </summary>
    public class ElementCell : UIElement
    {
        [Header("Element Cell")]
        [SerializeField, ReadOnly]
        private CellContent content;

        [SerializeField, RuntimeReadOnly]
        private SpriteRenderer[] sprites;

        private Color[] originalColors;

        public CellContent Content => content;
        public Action<ElementCell, CellContent> ContentSetter { get; set; }

        protected override void Awake()
        {
            base.Awake();

            originalColors = new Color[sprites.Length];
        }

        public void SetContent(CellContent content)
        {
            if (ContentSetter != null)
            {
                ContentSetter(this, content);
                return;
            }

            ForceSetContent(content);
        }

        public void ForceSetContent(CellContent content)
        {
            this.content = content;

            if (content == null)
                return;

            content.transform.SetParent(transform, true);
            content.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            content.Cell = this;
        }

        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < sprites.Length; i++)
                sprites[i].color = sprites[i].color.With(a: alpha);
        }

        public Tween FadeIn(ITweenData data)
        {
            for (int i = 0; i < sprites.Length; i++)
                originalColors[i] = sprites[i].color;

            void update(float t)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    var sprite = sprites[i];
                    sprite.color = Color.LerpUnclamped(originalColors[i], originalColors[i].With(a: 1.0f), t);
                }
            }

            return TweenManager.DoTween(update, data).WithLifetime(this);
        }

        public Tween FadeOut(ITweenData data)
        {
            for (int i = 0; i < sprites.Length; i++)
                originalColors[i] = sprites[i].color;

            void update(float t)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    var sprite = sprites[i];
                    sprite.color = Color.LerpUnclamped(originalColors[i], originalColors[i].With(a: 0.0f), t);
                }
            }

            return TweenManager.DoTween(update, data).WithLifetime(this);
        }
    }
}
