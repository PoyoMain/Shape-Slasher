using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Options SO", menuName = "Options")]
public class OptionsSO : ScriptableObject
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private bool controllerRumble;

    public AudioMixer MainAudioMixer => audioMixer;
    public bool ControllerRumble => controllerRumble;

    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";
    private const string MIXER_AMBIENCE = "AmbienceVolume";

    public void SetControllerRumble()
    {
        controllerRumble = !controllerRumble;
    }



    public void SetMasterVolume(Slider slider)
    {
        audioMixer.SetFloat(MIXER_MASTER, slider.value);
    }

    public void SetMusicVolume(Slider slider)
    {
        audioMixer.SetFloat(MIXER_MUSIC, slider.value);
    }

    public void SetSFXVolume(Slider slider)
    {
        audioMixer.SetFloat(MIXER_SFX, slider.value);
    }

    public void SetAmbienceVolume(Slider slider)
    {
        audioMixer.SetFloat(MIXER_AMBIENCE, slider.value);
    }
}
