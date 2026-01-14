using UnityEngine;

namespace Shears
{
    public static class AnimatorUtil
    {
        public static bool IsPlaying(this Animator animator, string animationName)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            return state.IsName(animationName) && (state.normalizedTime < 1 || state.loop);
        }
    }
}
