using Shears.Logging;
using Shears;
using UnityEngine;
using System;

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
        private AudioClip[] clips = Array.Empty<AudioClip>();

        [SerializeField]
        [Tooltip("Randomize the sound to play from the clips provided")]
        private bool randomizeSound;

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

        private readonly Timer preventOverlapTimer = new();

        public AudioSource Source
        {
            get => source;
            set => source = value;
        }
        public AudioClip[] Clip
        {
            get => clips;
            set => clips = value;
        }
        public bool RandomizeSound
        {
            get => randomizeSound;
            set => randomizeSound = value;
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

            if (clips == null)
            {
                this.LogError(
                    $"{nameof(AudioPlayerListener)} has no assigned {nameof(AudioClip)}!"
                );
                return;
            }

            int soundIndex = 0;
            if (randomizeSound)
            {
                soundIndex = UnityEngine.Random.Range(0, clips.Length);
            }

            source.clip = clips[soundIndex];

            float pitch = this.pitch;

            if (randomizePitch)
                pitch += randomPitchOffset.Random();

            source.pitch = pitch;
            source.volume = volume;

            if (!preventOverlapTimer.IsPlaying)
            {
                source.PlayOneShot(clips[soundIndex]);
                preventOverlapTimer.Start(.05f);
            }
        }
    }
}
