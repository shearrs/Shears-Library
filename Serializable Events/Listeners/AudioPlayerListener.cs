using Shears.Logging;
using UnityEngine;

namespace Shears.Events
{
    /// <summary>
    /// <see cref="SerializableListener"/> that plays an <see cref="AudioClip"/>.
    /// </summary>
    public class AudioPlayerListener : SerializableListener, ISHLoggable
    {
        [field: Header("Logging")]
        [field: SerializeField]
        public SHLogLevels LogLevels { get; set; } = SHLogUtil.Default;

        [Header("Listener")]
        [SerializeField]
        [Tooltip("The audio source to play sounds from.")]
        private AudioSource source;

        [SerializeField]
        [Tooltip("The audio clip to play.")]
        private AudioClip clip;

        [SerializeField, Range(-3, 3)]
        private float pitch = 1.0f;

        [SerializeField]
        [Tooltip("Whether or not to randomize the pitch when played.")]
        private bool randomizePitch = false;

        [SerializeField, ShowIf(nameof(randomizePitch))]
        [Tooltip("The range of randomization to the pitch.")]
        private Range<float> randomPitchOffset = new(-0.15f, 0.15f);

        [SerializeField, Range(0, 1)]
        private float volume = 1.0f;

        public AudioSource Source
        {
            get => source;
            set => source = value;
        }
        public AudioClip Clip
        {
            get => clip;
            set => clip = value;
        }
        public float Pitch
        {
            get => pitch;
            set => pitch = value;
        }
        public bool RandomizePitch
        {
            get => randomizePitch;
            set => randomizePitch = value;
        }
        public Range<float> RandomPitchOffset
        {
            get => randomPitchOffset;
            set => randomPitchOffset = value;
        }
        public float Volume
        {
            get => volume;
            set => volume = value;
        }

        protected override void OnInvoke()
        {
            if (source == null)
            {
                this.LogError(
                    $"{nameof(AudioPlayerListener)} has no assigned {nameof(AudioSource)}!"
                );
                return;
            }

            if (clip == null)
            {
                this.LogError(
                    $"{nameof(AudioPlayerListener)} has no assigned {nameof(AudioClip)}!"
                );
                return;
            }

            source.clip = clip;

            float pitch = this.pitch;

            if (randomizePitch)
                pitch += randomPitchOffset.Random();

            source.pitch = pitch;
            source.volume = volume;

            source.Play();
        }
    }
}
