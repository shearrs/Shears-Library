using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [System.Serializable]
    public class TweenData : ITweenData
    {
        [Header("Object")]
        [SerializeField] private bool useDataObject;
        [SerializeField, ShowIf("useDataObject")] private TweenDataObject tweenDataObject;

        [Header("Settings")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private bool forceFinalValue = true;
        [SerializeField] private int loops = 0;
        [SerializeField] private LoopMode loopMode = LoopMode.None;
        [SerializeField] private Ease easingFunction = Ease.Linear;

        public TweenData()
        {
            duration = 1.0f;
            forceFinalValue = true;
            loops = 0;
            loopMode = LoopMode.None;
            easingFunction = Ease.Linear;
        }

        public TweenData(float duration, bool forceFinalValue = true, int loops = 0, LoopMode loopMode = LoopMode.None, Ease easingFunction = Ease.Linear)
        {
            this.duration = duration;
            this.forceFinalValue = forceFinalValue;
            this.loops = loops;
            this.loopMode = loopMode;
            this.easingFunction = easingFunction;
        }

        public float Duration => HasDataObject() ? tweenDataObject.Duration : duration;
        public bool ForceFinalValue => HasDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public int Loops => HasDataObject() ? tweenDataObject.Loops : loops;
        public LoopMode LoopMode => HasDataObject() ? tweenDataObject.LoopMode : loopMode;
        public Ease EasingFunction => HasDataObject() ? tweenDataObject.EasingFunction : easingFunction;

        private bool HasDataObject() => tweenDataObject != null;
    }
}
