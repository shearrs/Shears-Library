using System.Collections.Generic;
using UnityEngine;
using static Shears.UI.InteractableGraphic;

namespace Shears.UI
{
    [System.Serializable]
    public class InteractableGraphicHandler
    {
        [SerializeField, Required(targetCollectionSize: 1)]
        private List<InteractableGraphic> graphics = new();

        private IReadOnlyRef<bool> selectable = new Ref<bool>(true);
        private IReadOnlyRef<bool> isHovered = new Ref<bool>(false);
        private IReadOnlyRef<bool> isFocused = new Ref<bool>(false);
        private IReadOnlyRef<bool> isDragged = new Ref<bool>(false);
        private IReadOnlyRef<bool> isPressed = new Ref<bool>(false);

        public void Validate()
        {
            foreach (var graphic in graphics)
                graphic.ValidateColors();
        }

        public void InitializeNotSelectable()
        {
            foreach (var graphic in graphics)
                graphic.InteractModulate = graphic.NotSelectableColor;
        }

        public void SetBaseColor(Color value)
        {
            foreach (var graphic in graphics)
                graphic.BaseColor = value;
        }

        public void SetModulate(Color value)
        {
            foreach (var graphic in graphics)
                graphic.Modulate = value;
        }

        public void AddGraphic(UIImage image)
        {
            graphics.Add(new(image));
        }

        public void AddGraphic(Renderer renderer)
        {
            graphics.Add(new(renderer));
        }

        public void AddGraphic(UIText text)
        {
            graphics.Add(new(text));
        }

        public void AddGraphic(UITextGUI text)
        {
            graphics.Add(new(text));
        }

        public void BindSelectable(IReadOnlyRef<bool> selectable)
        {
            this.selectable = selectable;
        }

        public void BindIsHovered(IReadOnlyRef<bool> isHovered)
        {
            this.isHovered = isHovered;
        }

        public void BindIsFocused(IReadOnlyRef<bool> isFocused)
        {
            this.isFocused = isFocused;
        }

        public void BindIsDragged(IReadOnlyRef<bool> isDragged)
        {
            this.isDragged = isDragged;
        }

        public void BindIsPressed(IReadOnlyRef<bool> isPressed)
        {
            this.isPressed = isPressed;
        }

        public void Update()
        {
            InteractColor newColor;

            if (!selectable.Value)
                newColor = InteractColor.NotSelectable;
            else
            {
                if (isDragged.Value)
                    newColor = InteractColor.Hover;
                else if (isPressed.Value)
                    newColor = isHovered.Value ? InteractColor.Press : InteractColor.None;
                else if (isHovered.Value)
                    newColor = InteractColor.Hover;
                else if (isFocused.Value)
                    newColor = InteractColor.Hover;
                else
                    newColor = InteractColor.None;
            }

            foreach (var graphic in graphics)
            {
                if (graphic.IsInteractColor(newColor))
                    continue;
                else if (graphic.TargetColor == newColor && graphic.IsMovingTowardsColor)
                    continue;

                graphic.MoveTowardsColor(newColor);
            }
        }
    }
}
