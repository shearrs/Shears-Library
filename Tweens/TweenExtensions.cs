using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.Tweens
{
    public static class TweenExtensions
    {
        private static Tween CreateTween(Action<float> update, ITweenData data) => TweenManager.CreateTween(update, data);
        private static Tween Do(Tween tween)
        {
            tween.Play();

            return tween;
        }

        private static Tween CreateAutoDisposeTween(UnityEngine.Object obj, Action<float> update, ITweenData data)
        {
            var tween = CreateTween(update, data);

            tween.AddDisposeEvent(GetAutoDisposeEvent(obj));

            return tween;
        }

        public static TweenStopEvent GetAutoDisposeEvent(UnityEngine.Object obj)
        {
            bool disposeEvent()
            {
                if (Application.isPlaying)
                    return obj == null;
                else
                    return false;
            }

            return disposeEvent;
        }

        #region Transform Tweens
        #region Move Tween
        public static Tween DoMoveTween(this Transform transform, Vector3 targetPos, ITweenData data = null) => Do(GetMoveTween(transform, targetPos, data));
        public static Tween GetMoveTween(this Transform transform, Vector3 targetPos, ITweenData data = null)
        {
            Vector3 start = transform.position;

            void update(float t) => transform.position = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Move Tween
        public static Tween DoMoveLocalTween(this Transform transform, Vector3 targetPos, ITweenData data = null) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static Tween GetMoveLocalTween(this Transform transform, Vector3 targetPos, ITweenData data = null)
        {
            Vector3 start = transform.localPosition;

            void update(float t) => transform.localPosition = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Rotate Tween
        public static Tween DoRotateTween(this Transform transform, Quaternion targetRot, bool shortestPath, ITweenData data = null) => Do(GetRotateTween(transform, targetRot, shortestPath, data));
        public static Tween GetRotateTween(this Transform transform, Quaternion targetRot, bool shortestPath, ITweenData data = null)
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
        public static Tween DoRotateLocalTween(this Transform transform, Quaternion targetRot, bool shortestPath, ITweenData data = null) => Do(GetRotateLocalTween(transform, targetRot, shortestPath, data));
        public static Tween GetRotateLocalTween(this Transform transform, Quaternion targetRot, bool shortestPath, ITweenData data = null)
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
        public static Tween DoScaleLocalTween(this Transform transform, Vector3 targetScale, ITweenData data = null) => Do(GetScaleLocalTween(transform, targetScale, data));
        public static Tween GetScaleLocalTween(this Transform transform, Vector3 targetScale, ITweenData data = null)
        {
            Vector3 start = transform.localScale;

            void update(float t) => transform.localScale = Vector3.LerpUnclamped(start, targetScale, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region Rigidbody Tweens
        #region Move Tween
        public static Tween DoMoveTween(this Rigidbody rb, Vector3 targetPos, ITweenData data = null) => Do(GetMoveTween(rb, targetPos, data));
        public static Tween GetMoveTween(this Rigidbody rb, Vector3 targetPos, ITweenData data = null)
        {
            Vector3 start = rb.position;

            void update(float t) => rb.position = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(rb, update, data);
        }
        #endregion

        #region Rotate Tween
        public static Tween DoRotateTween(this Rigidbody rb, Quaternion targetRot, bool shortestPath, ITweenData data = null) => Do(GetRotateTween(rb, targetRot, shortestPath, data));
        public static Tween GetRotateTween(this Rigidbody rb, Quaternion targetRot, bool shortestPath, ITweenData data = null)
        {
            Quaternion start = rb.rotation;

            Action<float> update;

            if (shortestPath)
                update = (t) => rb.rotation = Quaternion.LerpUnclamped(start, targetRot, t);
            else
            {
                update = (t) =>
                {
                    start.ToAngleAxis(out float sourceAngle, out Vector3 sourceAxis);
                    targetRot.ToAngleAxis(out float targetAngle, out Vector3 targetAxis);

                    float angle = Mathf.LerpUnclamped(sourceAngle, targetAngle, t);
                    Vector3 axis = Vector3.SlerpUnclamped(sourceAxis, targetAxis, t);

                    rb.rotation = Quaternion.AngleAxis(angle, axis);
                };
            }

            return CreateAutoDisposeTween(rb, update, data);
        }
        #endregion
        #endregion

        #region RectTransform Tweens
        #region Local Move Tween
        public static Tween DoMoveLocalTween(this RectTransform transform, Vector3 targetPos, ITweenData data = null) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static Tween GetMoveLocalTween(this RectTransform transform, Vector3 targetPos, ITweenData data = null)
        {
            Vector3 start = transform.anchoredPosition;

            void update(float t) => transform.anchoredPosition3D = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region Image Tweens
        public static Tween DoColorTween(this Image image, Color targetColor, ITweenData data = null) => Do(GetColorTween(image, targetColor, data));
        public static Tween GetColorTween(this Image image, Color targetColor, ITweenData data = null)
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
        public static Tween DoColorTween(this SpriteRenderer spriteRenderer, Color targetColor, ITweenData data = null) => Do(GetColorTween(spriteRenderer, targetColor, data));
        public static Tween GetColorTween(this SpriteRenderer spriteRenderer, Color targetColor, ITweenData data = null)
        {
            Color start = spriteRenderer.color;

            void update(float t)
            {
                spriteRenderer.color = Color.LerpUnclamped(start, targetColor, t);
            }

            return CreateAutoDisposeTween(spriteRenderer, update, data);
        }
        #endregion

        #region TextMesh Tweens
        public static Tween DoColorTween(this TextMeshProUGUI textMesh, Color targetColor, ITweenData data = null) => Do(GetColorTween(textMesh, targetColor, data));
        public static Tween GetColorTween(this TextMeshProUGUI textMesh, Color targetColor, ITweenData data = null)
        {
            Color start = textMesh.color;

            void update(float t)
            {
                textMesh.color = Color.LerpUnclamped(start, targetColor, t);
            }

            return CreateAutoDisposeTween(textMesh, update, data);
        }
        #endregion

        #region IColorTweenable Tweens
        public static Tween DoColorTween(this IColorTweenable colorTweenable, Color targetColor, ITweenData data = null) => Do(GetColorTween(colorTweenable, targetColor, data));
        public static Tween GetColorTween(this IColorTweenable colorTweenable, Color targetColor, ITweenData data = null)
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

        public static Tween DoColorMultTween(this IColorTweenable colorTweenable, Color baseColor, Color startColor, Color endColor, ITweenData data = null) => Do(GetColorMultTween(colorTweenable, baseColor, startColor, endColor, data));
        public static Tween GetColorMultTween(this IColorTweenable colorTweenable, Color baseColor, Color startColor, Color endColor, ITweenData data = null)
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
