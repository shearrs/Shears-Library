using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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

        private SpriteRenderer spriteRenderer;

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

        private void OnValidate()
        {
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite.color != modulate * baseColor)
                sprite.color = modulate * baseColor;
        }

        protected override Color GetBaseColor()
        {
            return baseColor;
        }

        protected override void SetBaseColor(Color color)
        {
            baseColor = color;
            SpriteRenderer.color = Modulate * baseColor;
        }

        protected override Color GetModulate()
        {
            return modulate;
        }

        protected override void SetModulate(Color color)
        {
            modulate = color;
            SpriteRenderer.color = Modulate * baseColor;
        }
    }
}
