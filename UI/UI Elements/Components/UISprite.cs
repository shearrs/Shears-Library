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

        public override Color BaseColor
        {
            get => baseColor;
            set
            {
                baseColor = value;
                SpriteRenderer.color = Modulate * baseColor;
            }
        }

        public override Color Modulate
        {
            get => modulate;
            set
            {
                modulate = value;
                SpriteRenderer.color = Modulate * baseColor;
            }
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
            {
                Debug.Log("change color");
                sprite.color = modulate * baseColor;
            }
        }
    }
}
