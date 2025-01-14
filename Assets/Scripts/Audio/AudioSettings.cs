using System;
using UnityEngine;
using UnityEngine.Audio;
using Slider = UnityEngine.UI.Slider;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private AudioMixer mixer;
    
    private float _masterVolume;
    private float _sfxVolume;
    private float _musicVolume;

    public float MasterVolume
    {
        get => _masterVolume;
        private set
        {
            _masterVolume = value;
            mixer.SetFloat("Master Volume", Mathf.Log10(_masterVolume) * 20);
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        private set
        {
            _sfxVolume = value;
            mixer.SetFloat("Sfx Volume", Mathf.Log10(_sfxVolume) * 20);
            PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        private set
        {
            _musicVolume = value;
            mixer.SetFloat("Music Volume", Mathf.Log10(_musicVolume) * 20);
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        }
    }
    
    
    private void Start()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume");
        SfxVolume = PlayerPrefs.GetFloat("SfxVolume");
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        masterVolumeSlider.value = MasterVolume;
        sfxVolumeSlider.value = SfxVolume;
        musicVolumeSlider.value = MusicVolume;
    }

    public void UpdateVolumes()
    {
        MasterVolume = masterVolumeSlider.value;
        SfxVolume = sfxVolumeSlider.value;
        MusicVolume = musicVolumeSlider.value;
    }
}
