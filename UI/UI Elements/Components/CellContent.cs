using Shears.Tweens;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shears.UI
{
    [RequireComponent(typeof(UIElement))]
    public partial class CellContent : MonoBehaviour
    {
        [SerializeField]
        private new Renderer renderer;

        [SerializeField]
        private ElementCell cell;

        [SerializeField, RuntimeReadOnly]
        private bool isDraggable = true;

        [SerializeField, RuntimeReadOnly, Min(0.0f)]
        private float dragBeginTime = 0.1f;

        [SerializeField]
        private ColorModulator colorModulator;

        [Auto]
        private UIElement element;
        private Vector3 offset;
        private bool initialized = false;

        public Renderer Renderer => renderer;
        public ElementCell Cell { get => cell; set => cell = value; }
        public ColorModulator ColorModulator => colorModulator;

        public event Action DragBegan;
        public event Action DragEnded;

        private void Awake()
        {
            __AutoAwake();

            element.DragBeginTime = dragBeginTime;

            if (cell != null)
                cell.SetContent(this);
        }

        private void Start()
        {
            colorModulator = new(element, renderer);

            if (isDraggable)
                EnableDrag();

            initialized = true;
        }

        public void SetAlpha(float alpha)
        {
            var mat = renderer.material;

            mat.color = mat.color.With(a: alpha);
        }

        public Tween FadeIn(ITweenData data)
        {
            var material = renderer.material;

            return material.DoColorTween(material.color.With(a: 1.0f), data);
        }

        public Tween FadeOut(ITweenData data)
        {
            var material = renderer.material;

            return material.DoColorTween(material.color.With(a: 0.0f), data);
        }

        public void EnableDrag()
        {
            if (isDraggable && initialized)
                return;

            element.RegisterEvent<DragBeginEvent>(OnDragBegin);
            element.RegisterEvent<DragEvent>(OnDrag);
            element.RegisterEvent<DragEndEvent>(OnDragEnd);

            isDraggable = true;
        }

        public void DisableDrag()
        {
            if (!isDraggable)
                return;

            element.DeregisterEvent<DragBeginEvent>(OnDragBegin);
            element.DeregisterEvent<DragEvent>(OnDrag);
            element.DeregisterEvent<DragEndEvent>(OnDragEnd);

            isDraggable = false;
        }

        public void ResetToCell()
        {
            if (cell != null)
                cell.SetContent(this);
        }

        private void OnDragBegin(DragBeginEvent evt)
        {
            evt.PreventTrickleDown();

            colorModulator.TweenToPressed();
            colorModulator.CanChangeColor = false;

            offset = evt.PointerWorldOffset;

            transform.SetParent(null);

            DragBegan?.Invoke();
        }

        private void OnDrag(DragEvent evt)
        {
            evt.PreventTrickleDown();

            const float MOVE_SPEED = 8.0f;

            Vector3 pointerWorld = evt.PointerWorldPosition;
            Vector3 targetPosition = pointerWorld + offset;
            element.transform.position = Vector3.MoveTowards(element.transform.position, targetPosition, MOVE_SPEED * Time.deltaTime);
        }

        private void OnDragEnd(DragEndEvent evt)
        {
            evt.PreventTrickleDown();

            colorModulator.CanChangeColor = true;

            if (colorModulator.IsHovered)
                colorModulator.TweenToHover();
            else
                colorModulator.ClearModulation();

            if (UIElementEventSystem.TryRaycastElement(out ElementCell newCell))
            {
                if (cell != null)
                    cell.SetContent(null);

                newCell.SetContent(this);
            }
            else if (cell != null)
                ResetToCell();

            DragEnded?.Invoke();
        }
    }
}
