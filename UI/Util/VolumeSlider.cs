using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Shears.UI
{
    public class VolumeSlider : MonoBehaviour
    {
        [Header("System Reference")]
        [SerializeField, RequiredField] private Slider slider;

        [Header("Audio")]
        [SerializeField] private string prefKey = "Volume";
        [SerializeField, RequiredField] private AudioMixerGroup mixerGroup;

        private float previousValue;

        private void OnEnable()
        {
            SetVolume();

            previousValue = slider.value;
        }

        private void Update()
        {
            UpdatePref();
        }

        private void UpdatePref()
        {
            if (slider.value != previousValue)
            {
                float volume = Mathf.Log10(slider.value) * 20f;

                PlayerPrefs.SetFloat(prefKey, volume);
            }

            previousValue = slider.value;
        }

        private void SetVolume()
        {
            float volume = PlayerPrefs.GetFloat(prefKey, -1);

            if (volume == -1)
                volume = 0;

            mixerGroup.audioMixer.SetFloat(prefKey, volume);
        }
    }
}
