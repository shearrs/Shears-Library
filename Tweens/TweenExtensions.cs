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

        private static ITween CreateAutoDisposeTween(Transform transform, Action<float> update, TweenData data)
        {
            var tween = CreateTween(update, data);

            bool disposeEvent()
            {
                if (Application.isPlaying)
                    return transform == null;
                else
                    return false;
            }

            tween.AddDisposeEvent(disposeEvent);

            return tween;
        }

        #region Transform Tweens
        #region Move Tween
        public static ITween DoMoveTween(this Transform transform, Vector3 targetPos, TweenData data) => Do(GetMoveTween(transform, targetPos, data));
        public static ITween GetMoveTween(this Transform transform, Vector3 targetPos, TweenData data)
        {
            Vector3 start = transform.position;

            void update(float t) => transform.position = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Move Tween
        public static ITween DoMoveLocalTween(this Transform transform, Vector3 targetPos, TweenData data) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static ITween GetMoveLocalTween(this Transform transform, Vector3 targetPos, TweenData data)
        {
            Vector3 start = transform.localPosition;

            void update(float t) => transform.localPosition = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Rotate Tween
        public static ITween DoRotateTween(this Transform transform, Quaternion targetRot, TweenData data) => Do(GetRotateTween(transform, targetRot, data));
        public static ITween GetRotateTween(this Transform transform, Quaternion targetRot, TweenData data)
        {
            Quaternion start = transform.rotation;

            void update(float t) => transform.rotation = Quaternion.LerpUnclamped(start, targetRot, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Rotate Tween
        public static ITween DoRotateLocalTween(this Transform transform, Quaternion targetRot, TweenData data) => Do(GetRotateLocalTween(transform, targetRot, data));
        public static ITween GetRotateLocalTween(this Transform transform, Quaternion targetRot, TweenData data)
        {
            Quaternion start = transform.localRotation;

            void update(float t) => transform.localRotation = Quaternion.LerpUnclamped(start, targetRot, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion

        #region Local Scale Tween
        public static ITween DoScaleLocalTween(this Transform transform, Vector3 targetScale, TweenData data) => Do(GetScaleLocalTween(transform, targetScale, data));
        public static ITween GetScaleLocalTween(this Transform transform, Vector3 targetScale, TweenData data)
        {
            Vector3 start = transform.localScale;

            void update(float t) => transform.localScale = Vector3.LerpUnclamped(start, targetScale, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region RectTransform Tweens
        #region Local Move Tween
        public static ITween DoMoveLocalTween(this RectTransform transform, Vector3 targetPos, TweenData data) => Do(GetMoveLocalTween(transform, targetPos, data));
        public static ITween GetMoveLocalTween(this RectTransform transform, Vector3 targetPos, TweenData data)
        {
            Vector3 start = transform.anchoredPosition;

            void update(float t) => transform.anchoredPosition = Vector3.LerpUnclamped(start, targetPos, t);

            return CreateAutoDisposeTween(transform, update, data);
        }
        #endregion
        #endregion

        #region Image Tweens
        public static ITween DoColorTween(this Image image, Color targetColor, TweenData data) => Do(GetColorTween(image, targetColor, data));
        public static ITween GetColorTween(this Image image, Color targetColor, TweenData data)
        {
            Color start = image.color;

            void update(float t) => image.color = Color.LerpUnclamped(start, targetColor, t);

            return CreateAutoDisposeTween(image.transform, update, data);
        }
        #endregion
    }
}
