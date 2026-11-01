using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;

namespace Shears.Audio
{
    public class MusicGroup
    {
        private readonly IReadOnlyList<MusicClip> clips;
        private readonly IReadOnlyList<AudioSource> sources;

        internal IReadOnlyList<AudioSource> Sources => sources;
        public IReadOnlyList<MusicClip> Clips => clips;
        public bool IsActive { get; internal set; }

        internal MusicGroup(IReadOnlyList<MusicClip> clips, IReadOnlyList<AudioSource> sources)
        {
            this.clips = clips;
            this.sources = sources;

            for (int i = 0; i < clips.Count; i++)
            {
                var musicClip = clips[i];
                var source = sources[i];

                source.clip = musicClip.Clip;
                source.volume = musicClip.Volume;
                source.loop = musicClip.Loop;
            }
        }

        public void PlayAutomaticClips()
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].AutomaticallyPlay)
                    sources[i].Play();
            }
        }

        public void Play(int index)
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            sources[index].Play();
        }

        public void Stop(int index)
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            sources[index].Stop();
        }

        public void StopAll()
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            foreach (var source in sources)
                source.Stop();
        }

        public void SetVolume(int index, float volume)
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            sources[index].volume = volume;
        }

        public void LowerAllVolume(float volume)
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            foreach (var source in sources)
                source.volume = Mathf.Min(source.volume, volume);
        }

        public void SetVolumePercentage(float percentage)
        {
            if (!IsActive)
            {
                LogActiveError();
                return;
            }

            for (int i = 0; i < sources.Count; i++)
            {
                var clip = clips[i];
                sources[i].volume = Mathf.Lerp(0, clip.Volume, percentage);
            }
        }

        private void LogActiveError() =>
            SHLogger.LogError($"Cannot call methods on an inactive {nameof(MusicGroup)}.");
    }
}
