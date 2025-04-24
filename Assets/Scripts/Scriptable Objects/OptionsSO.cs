using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Options SO", menuName = "Options")]
public class OptionsSO : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private bool controllerRumble;
    [SerializeField] private bool speedRunMode;
    [SerializeField] private Difficulty difficulty;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    public bool ControllerRumble => controllerRumble;
    public bool SpeedrunMode => speedRunMode;
    public Difficulty Difficulty => difficulty;
    public AudioMixer MainAudioMixer => audioMixer;

    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";
    private const string MIXER_AMBIENCE = "AmbienceVolume";

    public void SetControllerRumble()
    {
        controllerRumble = !controllerRumble;
    }

    public void SetSpeedRunMode()
    {
        speedRunMode = !speedRunMode;
    }

    public void SetSpeedRunMode(bool value)
    {
        speedRunMode = value;
    }

    public void SetEasyDifficulty()
    {
        difficulty = Difficulty.Easy;
    }

    public void SetMediumDifficulty()
    {
        difficulty = Difficulty.Medium;
    }

    public void SetHardDifficulty()
    {
        difficulty = Difficulty.Hard;
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

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
