using UnityEngine;

namespace Shears
{
    public static class AudioExtensions
    {
        public static void PlayWithRange(this AudioSource source, float minRange = 0.85f, float maxRange = 1.15f)
        {
            source.pitch = Random.Range(minRange, maxRange);
            source.Play();
        }
    }
}
