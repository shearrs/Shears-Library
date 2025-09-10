using UnityEngine;
using UnityEngine.Events;

namespace Shears.Tweens
{
    [System.Serializable]
    public class TweenUnityEvent : TweenEventBase
    {
        [SerializeField, Range(0f, 1f)] private float progress = 0.5f;
        [SerializeField] private UnityEvent events;

        public override bool CanInvoke(float t)
        {
            return t >= progress;
        }

        public override void Invoke()
        {
            events.Invoke();
        }
    }
}
