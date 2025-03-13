using UnityEngine;

namespace Shears.Tweens
{
    public class TransformTweener : MonoBehaviour
    {
        private enum TweenType { Move, LocalMove, Rotate, LocalRotate, LocalScale }

        [Header("Data")]
        [SerializeField] private TweenData data;
        [SerializeField] private TweenType type;
        [SerializeField] private bool playOnEnable;
        [SerializeField] private bool useRectTransform;
        private ITween tween;
        private RectTransform rectTransform;

        [Header("Input")]
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale = Vector3.one;

        [Header("Initialization")]
        [SerializeField] private bool initializeOnEnable;
        [SerializeField] private bool localInitialization = true;
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Vector3 initialRotation;
        [SerializeField] private Vector3 initialScale = Vector3.one;

        private void OnEnable()
        {
            if (useRectTransform)
                rectTransform = GetComponent<RectTransform>();

            if (initializeOnEnable)
                SetInitialValues();

            if (playOnEnable)
                Play();
        }

        private void OnDisable() => Stop();

        public void Play()
        {
            Stop();

            SetInitialValues();
            tween = GetTween();
            tween.Play();
        }

        public void Stop()
        {
            tween?.Stop();
            tween?.Dispose();
        }

        private ITween GetTween()
        {
            if (useRectTransform)
            {
                return type switch
                {
                    TweenType.Move => rectTransform.GetMoveTween(position, data),
                    TweenType.LocalMove => rectTransform.GetMoveLocalTween(position, data),
                    TweenType.Rotate => rectTransform.GetRotateTween(Quaternion.Euler(rotation), false, data),
                    TweenType.LocalRotate => rectTransform.GetRotateLocalTween(Quaternion.Euler(rotation), false, data),
                    TweenType.LocalScale => rectTransform.GetScaleLocalTween(scale, data),
                    _ => null,
                };
            }
            else
            {
                return type switch
                {
                    TweenType.Move => transform.GetMoveTween(position, data),
                    TweenType.LocalMove => transform.GetMoveLocalTween(position, data),
                    TweenType.Rotate => transform.GetRotateTween(Quaternion.Euler(rotation), false, data),
                    TweenType.LocalRotate => transform.GetRotateLocalTween(Quaternion.Euler(rotation), false, data),
                    TweenType.LocalScale => transform.GetScaleLocalTween(scale, data),
                    _ => null,
                };
            }
        }

        public void SetInitialValues()
        {
            if (useRectTransform)
                rectTransform.anchoredPosition = initialPosition;
            else
            {
                if (localInitialization)
                    transform.localPosition = initialPosition;
                else
                    transform.position = initialPosition;
            }

            if (localInitialization)
                transform.localRotation = Quaternion.Euler(initialRotation);
            else
                transform.rotation = Quaternion.Euler(initialRotation);

            transform.localScale = initialScale;
        }
    }
}
