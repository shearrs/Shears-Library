using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [System.Serializable]
    public struct StructTweenData : ITweenData
    {
        [Header("Tween Data Settings")]
        [SerializeField] private bool useDataObject;
        [SerializeField, ShowIf("useDataObject")] private TweenDataObject tweenDataObject;
        [SerializeField, ShowIf("!useDataObject")] private float duration;
        [SerializeField, ShowIf("!useDataObject")] private bool forceFinalValue;
        [SerializeField, ShowIf("!useDataObject")] private int loops;
        [SerializeField, ShowIf("!useDataObject")] private LoopMode loopMode;
        [SerializeField, ShowIf("!useDataObject")] private Ease easingFunction;

        public readonly float Duration => UsesDataObject() ? tweenDataObject.Duration : duration;
        public readonly bool ForceFinalValue => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public readonly int Loops => UsesDataObject() ? tweenDataObject.Loops : loops;
        public readonly LoopMode LoopMode => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
        public readonly Ease EasingFunction => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;

        public StructTweenData(float duration = 1.0f, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, Ease easingFunction = Ease.Linear)
        {
            useDataObject = false;
            tweenDataObject = null;

            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            this.easingFunction = easingFunction;
        }

        public void SetDataObject(TweenDataObject dataObject)
        {
            tweenDataObject = dataObject;
            useDataObject = dataObject != null;
        }

        private readonly bool UsesDataObject() => useDataObject && tweenDataObject != null;
    }
}
