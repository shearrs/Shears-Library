using System;
using System.Collections.Generic;
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
        [Tooltip("The audio clips to play.")]
        private AudioClip[] clips = Array.Empty<AudioClip>();

        [SerializeField]
        [Tooltip("Whether or not the randomize which sound clip plays.")]
        private bool randomizeClip = false;

        [SerializeField, Range(-3, 3)]
        [Tooltip("The pitch of the sound clip.")]
        private float pitch = 1.0f;

        [SerializeField]
        [Tooltip("Whether or not to randomize the pitch when played.")]
        private bool randomizePitch = false;

        [SerializeField, ShowIf(nameof(randomizePitch))]
        [Tooltip("The range of randomization to the pitch.")]
        private Range<float> randomPitchOffset = new(-0.15f, 0.15f);

        [SerializeField, Range(0, 1)]
        [Tooltip("The volume of the sound clip.")]
        private float volume = 1.0f;

        public AudioSource Source
        {
            get => source;
            set => source = value;
        }
        public IReadOnlyList<AudioClip> Clips => clips;
        public bool RandomizeClip
        {
            get => randomizeClip;
            set => randomizeClip = value;
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

            if (clips == null || clips.Length == 0)
            {
                this.LogError(
                    $"{nameof(AudioPlayerListener)} has no assigned {nameof(AudioClip)}!"
                );
                return;
            }

            int clipIndex = 0;

            if (randomizeClip)
                clipIndex = UnityEngine.Random.Range(0, clips.Length);

            source.clip = clips[clipIndex];

            float pitch = this.pitch;

            if (randomizePitch)
                pitch += randomPitchOffset.Random();

            source.pitch = pitch;
            source.volume = volume;

            source.Play();
        }
    }
}