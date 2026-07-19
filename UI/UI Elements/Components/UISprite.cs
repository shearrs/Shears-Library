using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class UISprite : UIElement
    {
        [SerializeField]
        private Color baseColor = Color.white;

        [SerializeField]
        private Color modulate = Color.white;

        [SerializeField]
        private bool additiveModulate;

        private SpriteRenderer spriteRenderer;

        protected override Color BaseColorValue
        {
            get => baseColor;
            set => baseColor = value;
        }
        protected override Color ModulateValue
        {
            get => modulate;
            set => modulate = value;
        }
        protected override bool AdditiveModulateValue
        {
            get => additiveModulate;
            set => additiveModulate = value;
        }

        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                    spriteRenderer = GetComponent<SpriteRenderer>();

                return spriteRenderer;
            }
        }

        public Sprite Sprite
        {
            get => SpriteRenderer.sprite;
            set => SpriteRenderer.sprite = value;
        }

        private void Reset()
        {
            var sprite = GetComponent<SpriteRenderer>();
            baseColor = sprite.color;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (Application.isPlaying)
                return;

            var sprite = GetComponent<SpriteRenderer>();
            var targetColor = AdditiveModulate
                ? (modulate + baseColor).With(a: Alpha)
                : (modulate * baseColor).With(a: Alpha);

            if (sprite.color != targetColor)
                sprite.color = targetColor;
        }

        protected override void Repaint(StyleData data)
        {
            SpriteRenderer.color = data.Color;
        }
    }
}
