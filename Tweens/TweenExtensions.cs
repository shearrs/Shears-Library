using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.Tweens
{
    public static class TweenExtensions
    {
        private static ITween CreateTween(Action<float> update, TweenData data) => TweenManager.CreateTween(update, data);
        private static ITween Do(ITween tween)
        {
            tween.Play();

            return tween;
        }

        private static ITween CreateAutoDisposeTween(UnityEngine.Object obj, Action<float> update, TweenData data)
        {
            var tween = CreateTween(update, data);

            bool disposeEvent()
            {
                if (Application.isPlaying)
                    return obj == null;
                else
                    return false;
            }

            tween.AddDisposeEvent(disposeEvent);

            return tween;
        }

        #region Transform Tweens
        #region Move Tween
        public static ITween DoMoveTween(this Transform transform, Vector3 targetPos, TweenData data = null) => Do(GetMoveTween(transform, targetPos, data));
        public static ITween GetMoveTween(this Transform transform, Vector3 targetPos, TweenData data = null)
        {
            Vector3 start = transform.position;

            void update(float t) => transform.position = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Move Tween
        public static ITween DoMoveLocalTween(this Transform transform, Vector3 targetPos, TweenData data = null) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static ITween GetMoveLocalTween(this Transform transform, Vector3 targetPos, TweenData data = null)
        {
            Vector3 start = transform.localPosition;

            void update(float t) => transform.localPosition = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Rotate Tween
        public static ITween DoRotateTween(this Transform transform, Quaternion targetRot, bool shortestPath, TweenData data = null) => Do(GetRotateTween(transform, targetRot, shortestPath, data));
        public static ITween GetRotateTween(this Transform transform, Quaternion targetRot, bool shortestPath, TweenData data = null)
        {
            Quaternion start = transform.rotation;

            Action<float> update;

            if (shortestPath)
                update = (t) => transform.rotation = Quaternion.LerpUnclamped(start, targetRot, t);
            else
            {
                update = (t) =>
                {
                    start.ToAngleAxis(out float sourceAngle, out Vector3 sourceAxis);
                    targetRot.ToAngleAxis(out float targetAngle, out Vector3 targetAxis);

                    float angle = Mathf.LerpUnclamped(sourceAngle, targetAngle, t);
                    Vector3 axis = Vector3.SlerpUnclamped(sourceAxis, targetAxis, t);

                    transform.rotation = Quaternion.AngleAxis(angle, axis);
                };
            }

                return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Rotate Tween
        public static ITween DoRotateLocalTween(this Transform transform, Quaternion targetRot, bool shortestPath, TweenData data = null) => Do(GetRotateLocalTween(transform, targetRot, shortestPath, data));
        public static ITween GetRotateLocalTween(this Transform transform, Quaternion targetRot, bool shortestPath, TweenData data = null)
        {
            Quaternion start = transform.localRotation;

            Action<float> update;

            if (shortestPath)
                update = (t) => transform.localRotation = Quaternion.LerpUnclamped(start, targetRot, t);
            else
            {
                update = (t) =>
                {
                    start.ToAngleAxis(out float sourceAngle, out Vector3 sourceAxis);
                    targetRot.ToAngleAxis(out float targetAngle, out Vector3 targetAxis);

                    float angle = Mathf.LerpUnclamped(sourceAngle, targetAngle, t);
                    Vector3 axis = Vector3.SlerpUnclamped(sourceAxis, targetAxis, t);

                    transform.localRotation = Quaternion.AngleAxis(angle, axis);
                };
            }

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Scale Tween
        public static ITween DoScaleLocalTween(this Transform transform, Vector3 targetScale, TweenData data = null) => Do(GetScaleLocalTween(transform, targetScale, data));
        public static ITween GetScaleLocalTween(this Transform transform, Vector3 targetScale, TweenData data = null)
        {
            Vector3 start = transform.localScale;

            void update(float t) => transform.localScale = Vector3.LerpUnclamped(start, targetScale, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region RectTransform Tweens
        #region Local Move Tween
        public static ITween DoMoveLocalTween(this RectTransform transform, Vector3 targetPos, TweenData data = null) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static ITween GetMoveLocalTween(this RectTransform transform, Vector3 targetPos, TweenData data = null)
        {
            Vector3 start = transform.anchoredPosition;

            void update(float t) => transform.anchoredPosition3D = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region Image Tweens
        public static ITween DoColorTween(this Image image, Color targetColor, TweenData data = null) => Do(GetColorTween(image, targetColor, data));
        public static ITween GetColorTween(this Image image, Color targetColor, TweenData data = null)
        {
            Color start = image.color;

            void update(float t)
            {
                image.color = Color.LerpUnclamped(start, targetColor, t);
            }

            return CreateAutoDisposeTween(image, update, data);
        }
        #endregion

        #region SpriteRenderer Tweens
        public static ITween DoColorTween(this SpriteRenderer spriteRenderer, Color targetColor, TweenData data = null) => Do(GetColorTween(spriteRenderer, targetColor, data));
        public static ITween GetColorTween(this SpriteRenderer spriteRenderer, Color targetColor, TweenData data = null)
        {
            Color start = spriteRenderer.color;

            void update(float t)
            {
                spriteRenderer.color = Color.LerpUnclamped(start, targetColor, t);
            }

            return CreateAutoDisposeTween(spriteRenderer, update, data);
        }
        #endregion

        #region IColorTweenable Tweens
        public static ITween DoColorTween(this IColorTweenable colorTweenable, Color targetColor, TweenData data = null) => Do(GetColorTween(colorTweenable, targetColor, data));
        public static ITween GetColorTween(this IColorTweenable colorTweenable, Color targetColor, TweenData data = null)
        {
            Color start = colorTweenable.CurrentColor;

            void update(float t)
            {
                colorTweenable.CurrentColor = Color.LerpUnclamped(start, targetColor, t);
            }

            if (colorTweenable is UnityEngine.Object unityObject)
                return CreateAutoDisposeTween(unityObject, update, data);
            else
                return CreateTween(update, data);
        }

        public static ITween DoColorMultTween(this IColorTweenable colorTweenable, Color baseColor, Color startColor, Color endColor, TweenData data = null) => Do(GetColorMultTween(colorTweenable, baseColor, startColor, endColor, data));
        public static ITween GetColorMultTween(this IColorTweenable colorTweenable, Color baseColor, Color startColor, Color endColor, TweenData data = null)
        {
            void update(float t)
            {
                colorTweenable.CurrentColor = baseColor * Color.LerpUnclamped(startColor, endColor, t);
            }

            if (colorTweenable is UnityEngine.Object unityObject)
                return CreateAutoDisposeTween(unityObject, update, data);
            else
                return CreateTween(update, data);
        }
        #endregion
    }
}
