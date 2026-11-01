using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace Shears.Audio
{
    public class MusicManager : ProtectedSingleton<MusicManager>
    {
        private readonly ObjectPool<AudioSource> audioSourcePool = new(
            CreateAudioSource,
            GetAudioSource,
            ReleaseAudioSource
        );
        private readonly Dictionary<CancellationTokenSource, MusicGroup> fadeOutGroups = new();
        private MusicGroup activeGroup;
        private CancellationTokenSource fadeInTokenSource;

        private void OnDestroy()
        {
            fadeInTokenSource?.Cancel();
            fadeInTokenSource?.Dispose();

            foreach (var source in fadeOutGroups.Keys)
            {
                source?.Cancel();
                source?.Dispose();
            }
        }

        public static MusicGroup Play(params MusicClip[] clips) => Instance.InstPlay(clips);

        private MusicGroup InstPlay(IReadOnlyList<MusicClip> clips)
        {
            var group = CreateMusicGroup(clips);

            if (activeGroup != null)
                CleanUpActiveGroup();

            activeGroup = group;
            activeGroup.PlayAutomaticClips();
            activeGroup.IsActive = true;

            return group;
        }

        public static async Awaitable<MusicGroup> CrossFade(
            float duration,
            params MusicClip[] clips
        ) => await Instance.CrossFadeAsync(duration, clips);

        private async Awaitable<MusicGroup> CrossFadeAsync(
            float duration,
            IReadOnlyList<MusicClip> clips
        )
        {
            var group = CreateMusicGroup(clips);

            if (activeGroup != null)
            {
                var source = new CancellationTokenSource();
                fadeOutGroups[source] = activeGroup;
                FadeOutAsync(activeGroup, duration, source);
            }

            fadeInTokenSource?.Cancel();
            fadeInTokenSource?.Dispose();
            fadeInTokenSource = new();

            activeGroup = group;
            activeGroup.SetVolumePercentage(0.0f);
            activeGroup.PlayAutomaticClips();
            activeGroup.IsActive = true;

            await FadeInAsync(group, duration, fadeInTokenSource.Token);

            return group;
        }

        private async void FadeOutAsync(
            MusicGroup group,
            float duration,
            CancellationTokenSource tokenSource
        )
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                await Awaitable.NextFrameAsync(tokenSource.Token);

                if (tokenSource.Token.IsCancellationRequested)
                    return;

                elapsedTime += Time.deltaTime;

                float t = 1.0f - (elapsedTime / duration);
                group.SetVolumePercentage(t);
            }

            foreach (var source in group.Sources)
                audioSourcePool.Release(source);

            group.IsActive = false;
            fadeOutGroups.Remove(tokenSource);
        }

        private async Awaitable FadeInAsync(
            MusicGroup group,
            float duration,
            CancellationToken token
        )
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                await Awaitable.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                    return;

                elapsedTime += Time.deltaTime;

                float t = elapsedTime / duration;
                group.SetVolumePercentage(t);
            }
        }

        public static void Stop() => Instance.InstStop();

        private void InstStop()
        {
            if (activeGroup == null)
                return;

            CleanUpActiveGroup();
            CleanUpFadeOutGroups();
        }

        private void CleanUpActiveGroup()
        {
            activeGroup.StopAll();

            foreach (var source in activeGroup.Sources)
                audioSourcePool.Release(source);

            activeGroup.IsActive = false;
            activeGroup = null;
        }

        private void CleanUpFadeOutGroups()
        {
            foreach (var (source, group) in fadeOutGroups)
            {
                source?.Cancel();
                source?.Dispose();

                group.StopAll();

                foreach (var audioSource in group.Sources)
                    audioSourcePool.Release(audioSource);

                group.IsActive = false;
            }

            fadeOutGroups.Clear();
        }

        private static AudioSource CreateAudioSource()
        {
            var gameObject = new GameObject($"Audio Source {Instance.audioSourcePool.CountAll}");
            var source = gameObject.AddComponent<AudioSource>();

            return source;
        }

        private static void GetAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        private static void ReleaseAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }

        private MusicGroup CreateMusicGroup(IReadOnlyList<MusicClip> clips)
        {
            var sources = new List<AudioSource>();
            for (int i = 0; i < clips.Count; i++)
                sources[i] = audioSourcePool.Get();

            var group = new MusicGroup(clips, sources);

            return group;
        }
    }
}
