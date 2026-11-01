using UnityEngine;

namespace Shears.Audio
{
    [System.Serializable]
    public class MusicClip
    {
        [SerializeField]
        private bool automaticallyPlay = true;

        [SerializeField]
        private AudioClip clip;

        [SerializeField, Range(0, 1)]
        private float volume;

        [SerializeField]
        private bool loop;

        public bool AutomaticallyPlay => automaticallyPlay;
        public AudioClip Clip => clip;
        public float Volume => volume;
        public bool Loop => loop;

        public MusicClip(
            AudioClip clip,
            float volume = 1.0f,
            bool loop = true,
            bool automaticallyPlay = true
        )
        {
            this.clip = clip;
            this.volume = volume;
            this.loop = loop;
            this.automaticallyPlay = automaticallyPlay;
        }
    }
}
