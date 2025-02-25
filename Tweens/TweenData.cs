using UnityEngine;
using static Shears.Tweens.EasingFunction;

namespace Shears.Tweens
{
    [CreateAssetMenu(fileName = "New Tween Data", menuName = "Tween/Data")]
    public class TweenData : ScriptableObject
    {
        [field: SerializeField] public float Duration { get; private set; } = 1;
        [field: SerializeField] public bool ForceFinalValue { get; private set; } = true;
        [field: SerializeField] public int Loops { get; private set; } = 0;
        [field: SerializeField] public LoopMode LoopMode { get; private set; }
        [field: SerializeField] public Ease EasingFunction { get; private set; } = Ease.Linear;
    }
}
