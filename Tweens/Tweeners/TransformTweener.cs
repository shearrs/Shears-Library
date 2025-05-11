using UnityEngine;

namespace Shears.Tweens
{
    public class TransformTweener : MonoBehaviour
    {
        public enum OnEnableBehaviour { None, Play1To2, Play2To1 };
        public enum TweenType { Move, LocalMove, Rotate, LocalRotate, LocalScale }

        [Header("Settings")]
        [SerializeField] private OnEnableBehaviour onEnableBehaviour;
        [SerializeField] private TweenType type;

        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private TweenData data;

        [Header("1")]
        [SerializeField] private Vector3 position1;
        [SerializeField] private Vector3 rotation1;
        [SerializeField] private Vector3 scale1 = Vector3.one;

        [Header("2")]
        [SerializeField] private Vector3 position2;
        [SerializeField] private Vector3 rotation2;
        [SerializeField] private Vector3 scale2 = Vector3.one;

        private ITween tween;

        private struct TransformTweenData
        {
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public Vector3 Scale { get; set; }

            public TransformTweenData(Vector3 position, Vector3 rotation, Vector3 scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }
        }

        private void Awake()
        {
            if (target == null)
                target = transform;
        }

        private void OnEnable()
        {
            switch (onEnableBehaviour)
            {
                case OnEnableBehaviour.None:
                    break;
                case OnEnableBehaviour.Play1To2:
                    Play1To2();
                    break;
                case OnEnableBehaviour.Play2To1:
                    Play2To1();
                    break;
            }
        }

        private void OnDisable() => Stop();

        public void Play1To2()
        {
            var from = new TransformTweenData(position1, rotation1, scale1);
            var to = new TransformTweenData(position2, rotation2, scale2);

            Play(GetTween(from, to));
        }

        public void Play2To1()
        {
            var from = new TransformTweenData(position2, rotation2, scale2);
            var to = new TransformTweenData(position1, rotation1, scale1);

            Play(GetTween(from, to));
        }

        private void Play(ITween tween)
        {
            ClearTween();

            this.tween = tween;
            tween.Play();
        }

        public void Stop() => ClearTween();

        private void ClearTween()
        {
            tween?.Stop();
            tween?.Dispose();

            tween = null;
        }

        private ITween GetTween(TransformTweenData fromData, TransformTweenData toData)
        {
            ITween tween = null;

            switch (type)
            {
                case TweenType.Move:
                    target.position = fromData.Position;
                    tween = target.GetMoveTween(toData.Position, data);
                    break;
                case TweenType.LocalMove:
                    target.position = fromData.Position;
                    tween = target.GetMoveLocalTween(toData.Position, data);
                    break;
                case TweenType.Rotate:
                    target.rotation = Quaternion.Euler(fromData.Rotation);
                    tween = target.GetRotateTween(Quaternion.Euler(toData.Rotation), true, data);
                    break;
                case TweenType.LocalRotate:
                    target.rotation = Quaternion.Euler(fromData.Rotation);
                    tween = target.GetRotateLocalTween(Quaternion.Euler(toData.Rotation), true, data);
                    break;
                case TweenType.LocalScale:
                    target.localScale = fromData.Scale;
                    tween = target.GetScaleLocalTween(toData.Scale, data);
                    break;
            }

            return tween;
        }
    }
}
