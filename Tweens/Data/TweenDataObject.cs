using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [CreateAssetMenu(fileName = "New Tween Data", menuName = "Shears Library/Tweens/Data Object")]
    public class TweenDataObject : ScriptableObject, ITweenData
    {
        [field: Header("Playing")]
        [field: SerializeField] public float Duration { get; private set; } = 1;
        [field: SerializeField] public bool ForceFinalValue { get; private set; } = true;

        [field: Header("Loops")]
        [field: SerializeField] public int Loops { get; private set; } = 0;
        [field: SerializeField] public LoopMode LoopMode { get; private set; }

        [field: Header("Easing")]
        [field: SerializeField] public bool UsesCurve { get; private set; } = false;
        [field: SerializeField, ShowIf("!UsesCurve")] public Ease EasingFunction { get; private set; } = Ease.Linear;
        [field: SerializeField, ShowIf("UsesCurve")] public AnimationCurve Curve { get; private set; }
    }
}
