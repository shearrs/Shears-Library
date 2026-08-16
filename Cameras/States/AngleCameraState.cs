using System.Collections.Generic;
using Shears.Tweens;
using UnityEngine;

namespace Shears.Cameras
{
    public class AngleCameraState : CameraState
    {
        [field: Header("Horizontal")]
        [field: SerializeField, Min(0)]
        public float HorizontalRotationIncrement { get; set; } = 90.0f;

        [field: SerializeField]
        public TweenData HorizontalTweenData { get; private set; } = new();

        [field: Header("Vertical")]
        [field: SerializeField, Min(0)]
        public float VerticalRotationIncrement { get; set; } = 15.0f;

        [field: SerializeField]
        public TweenData VerticalTweenData { get; private set; } = new();

        private readonly List<Direction> inputQueue = new();
        bool isRunningInput = false;
        private Direction verticalDirection = Direction.Forward;
        private Tween horizontalTween;
        private Tween verticalTween;
        private float targetHorizontalAngle;

        protected override void OnEnter()
        {
            InitializeDirection();
        }

        protected override void OnExit() { }

        protected override void OnLateUpdate()
        {
            UpdateInput();
            ProcessInputQueue();
        }

        private void InitializeDirection()
        {
            var forward = CameraTransform.forward;
            float x = Mathf.Abs(forward.x);
            float y = Mathf.Abs(forward.y);
            float z = Mathf.Abs(forward.z);
            Vector3 targetDirection;

            if (x > y && x > z)
                targetDirection = forward.x > 0 ? Vector3.right : Vector3.left;
            else if (y > x && y > z)
                targetDirection = forward.y > 0 ? Vector3.up : Vector3.down;
            else
                targetDirection = forward.z > 0 ? Vector3.forward : Vector3.back;

            CameraTransform.forward = targetDirection;
            targetHorizontalAngle = CameraTransform.localEulerAngles.y;
        }

        private void UpdateInput()
        {
            if (GlobalData.RotateRight())
            {
                for (int i = inputQueue.Count - 1; i >= 0; i--)
                {
                    if (inputQueue[i] == Direction.Left)
                    {
                        inputQueue.RemoveAt(i);
                        return;
                    }
                }

                inputQueue.Add(Direction.Right);
            }
            else if (GlobalData.RotateLeft())
            {
                for (int i = inputQueue.Count - 1; i >= 0; i--)
                {
                    if (inputQueue[i] == Direction.Right)
                    {
                        inputQueue.RemoveAt(i);
                        return;
                    }
                }

                inputQueue.Add(Direction.Left);
            }

            if (GlobalData.RotateUp())
            {
                if (verticalDirection != Direction.Up)
                    RotateVertical(Direction.Up);
            }
            else if (GlobalData.RotateDown())
            {
                if (verticalDirection != Direction.Down)
                    RotateVertical(Direction.Down);
            }
            else if (verticalDirection != Direction.Forward)
                UndoVerticalRotation();
        }

        private void ProcessInputQueue()
        {
            if (isRunningInput || inputQueue.Count == 0)
                return;

            var input = inputQueue[0];
            inputQueue.RemoveAt(0);

            if (input == Direction.Right)
                RotateHorizontal(true);
            else if (input == Direction.Left)
                RotateHorizontal(false);
        }

        private void RotateHorizontal(bool positive)
        {
            horizontalTween.Dispose();
            isRunningInput = true;

            int modifier = positive ? 1 : -1;
            float startAngle = targetHorizontalAngle;
            targetHorizontalAngle += modifier * HorizontalRotationIncrement;

            horizontalTween = TweenManager
                .DoTween(
                    t =>
                    {
                        float angle = Mathf.LerpUnclamped(startAngle, targetHorizontalAngle, t);

                        if (angle > 360.0f)
                            angle -= 360.0f;
                        else if (angle < -360.0f)
                            angle += 360.0f;

                        CameraTransform.localRotation = Quaternion.Euler(
                            CameraTransform.localEulerAngles.With(y: angle)
                        );
                    },
                    HorizontalTweenData
                )
                .WithLifetime(this);

            horizontalTween.Stopped += () =>
            {
                isRunningInput = false;
            };
        }

        private void RotateVertical(Direction direction)
        {
            verticalTween.Dispose();

            int modifier = direction == Direction.Up ? -1 : 1;
            float startAngle = CameraTransform.localEulerAngles.x;

            if (startAngle > 180.0f)
                startAngle = -(360.0f - startAngle);

            float targetAngle = modifier * VerticalRotationIncrement;

            verticalTween = TweenManager
                .DoTween(
                    t =>
                    {
                        float angle = Mathf.LerpUnclamped(startAngle, targetAngle, t);

                        CameraTransform.localEulerAngles = CameraTransform.localEulerAngles.With(
                            x: angle
                        );
                    },
                    VerticalTweenData
                )
                .WithLifetime(this);

            verticalDirection = direction;
        }

        private void UndoVerticalRotation()
        {
            verticalTween.Dispose();

            int modifier = verticalDirection == Direction.Up ? -1 : 1;
            var flatCameraRotation = Quaternion.Euler(CameraTransform.eulerAngles.With(x: 0));
            float targetAngle = Vector3.SignedAngle(
                flatCameraRotation * Vector3.forward,
                CameraTransform.forward,
                CameraTransform.right
            );

            var returnRotation = Quaternion.Euler(CameraTransform.localEulerAngles.With(x: 0));

            verticalTween = TweenManager
                .DoTween(
                    t =>
                    {
                        float angle = Mathf.LerpUnclamped(targetAngle, 0, t);

                        CameraTransform.localEulerAngles = CameraTransform.localEulerAngles.With(
                            x: angle
                        );
                    },
                    VerticalTweenData
                )
                .WithLifetime(this);

            verticalDirection = Direction.Forward;
        }
    }
}
