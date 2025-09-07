using System;
using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [Serializable]
    public class TweenData : ITweenData
    {
        [Header("Tween Data Settings")]
        [SerializeField] private bool useDataObject;
        [SerializeField, ShowIf("useDataObject")] private TweenDataObject tweenDataObject;
        [SerializeField, ShowIf("!useDataObject")] private float duration = 1.0f;
        [SerializeField, ShowIf("!useDataObject")] private bool forceFinalValue = true;
        [SerializeField, ShowIf("!useDataObject")] private int loops = 0;
        [SerializeField, ShowIf("!useDataObject")] private LoopMode loopMode = LoopMode.None;
        [SerializeField, ShowIf("!useDataObject")] private Ease easingFunction = Ease.Linear;

        public float Duration => UsesDataObject() ? tweenDataObject.Duration : duration;
        public bool ForceFinalValue => UsesDataObject() ? tweenDataObject.ForceFinalValue : forceFinalValue;
        public int Loops => UsesDataObject() ? tweenDataObject.Loops : loops;
        public LoopMode LoopMode => UsesDataObject() ? tweenDataObject.LoopMode : loopMode;
        public Ease EasingFunction => UsesDataObject() ? tweenDataObject.EasingFunction : easingFunction;

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

        private bool UsesDataObject() => useDataObject && tweenDataObject != null;

        public void SetDataObject(TweenDataObject dataObject)
        {
            tweenDataObject = dataObject;
            useDataObject = dataObject != null;
        }
    }
}
