using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [System.Serializable]
    public struct StructTweenData : ITweenData
    {
        [Header("Tween Data Settings")]
        [SerializeField] private bool usesDataObject;
        [SerializeField, ShowIf("usesDataObject")] private TweenDataObject tweenDataObject;
        [SerializeField, ShowIf("!usesDataObject")] private float duration;
        [SerializeField, ShowIf("!usesDataObject")] private bool forceFinalValue;
        [SerializeField, ShowIf("!usesDataObject")] private int loops;
        [SerializeField, ShowIf("!usesDataObject")] private LoopMode loopMode;
        [SerializeField] private bool usesCurve;
        [SerializeField, ShowIf("!usesDataObject", "!usesCurve")] private Ease easingFunction;
        [SerializeField, ShowIf("!usesDataObject", "usesCurve")] private AnimationCurve curve;

        public readonly float Duration => UsesDataObject() ? tweenDataObject.Duration : duration;
        public readonly bool ForceFinalValue => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public readonly int Loops => UsesDataObject() ? tweenDataObject.Loops : loops;
        public readonly LoopMode LoopMode => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
        public readonly Ease EasingFunction => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;
        public readonly bool UsesCurve => usesCurve;
        public readonly AnimationCurve Curve => curve;

        public StructTweenData(float duration = 1.0f, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, Ease easingFunction = Ease.Linear)
        {
            usesDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            this.easingFunction = easingFunction;
            usesCurve = false;
            curve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        public StructTweenData(float duration = 1.0f, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, AnimationCurve curve = null)
        {
            usesDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            easingFunction = Ease.Linear;
            usesCurve = true;
            this.curve = curve;
        }

        public void SetDataObject(TweenDataObject dataObject)
        {
            tweenDataObject = dataObject;
            usesDataObject = dataObject != null;
        }

        private readonly bool UsesDataObject() => usesDataObject && tweenDataObject != null;
    }
}
