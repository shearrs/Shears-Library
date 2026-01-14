using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI
{
    public class VolumeSlider : MonoBehaviour
    {
        [Header("System Reference")]
        [SerializeField] private Slider slider;

        [Header("Audio")]
        [SerializeField] private string prefKey = "Volume";
        [SerializeField] private AudioMixerGroup mixerGroup;

        [Header("Events")]
        [SerializeField] private UnityEvent onVolumeChanged;

        private float previousValue;

        public float Value { get => slider.value; set => slider.value = value; }

        private void OnEnable()
        {
            SetVolume();
            SyncSlider();

            previousValue = slider.value;
        }

        private void Update()
        {
            UpdatePref();
        }

        private void UpdatePref()
        {
            float sliderValue = slider.value + 0.0001f;

            if (sliderValue != previousValue)
            {
                PlayerPrefs.SetFloat(prefKey, sliderValue);
                PlayerPrefs.Save();

                SetVolume();

                onVolumeChanged.Invoke();
            }

            previousValue = sliderValue;
        }

        private void SetVolume()
        {
            float volume = PlayerPrefs.GetFloat(prefKey, 1);
            volume = Mathf.Log10(volume) * 20f;

            mixerGroup.audioMixer.SetFloat(prefKey, volume);
        }

        private void SyncSlider()
        {
            float volume = PlayerPrefs.GetFloat(prefKey, 1);

            slider.value = volume;
        }
    }
}
