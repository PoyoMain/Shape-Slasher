using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private OptionsSO optionsSO;

    [Header("Scene Objects")]
    [SerializeField] private Slider audioSlider_Master;
    [SerializeField] private Slider audioSlider_Music;
    [SerializeField] private Slider audioSlider_SFX;
    [SerializeField] private Slider audioSlider_Ambience;
    [Space(10)]
    [SerializeField] private Toggle checkbox_ControllerRumble;
    [SerializeField] private Toggle checkbox_Speedrun;
    [Space(10)]
    [SerializeField] private Toggle toggle_EasyDifficulty;
    [SerializeField] private Toggle toggle_MediumDifficulty;
    [SerializeField] private Toggle toggle_HardDifficulty;

    [SerializeField] private UnityEvent onOptionsClose;

    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private const string MIXER_MASTER = "MasterVolume";
    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";
    private const string MIXER_AMBIENCE = "AmbienceVolume";
    private const float VOLUME_MAX = 20;
    private const float VOLUME_MIN = -80f;

    private void Start()
    {
        RefreshAllElements();
    }

    public void RefreshAllElements()
    {
        audioSlider_Master.maxValue = audioSlider_Music.maxValue = audioSlider_SFX.maxValue = audioSlider_Ambience.maxValue = VOLUME_MAX;
        audioSlider_Master.minValue = audioSlider_Music.minValue = audioSlider_SFX.minValue = audioSlider_Ambience.minValue = VOLUME_MIN;

        optionsSO.MainAudioMixer.GetFloat(MIXER_MASTER, out float volume);
        audioSlider_Master.value = volume;

        optionsSO.MainAudioMixer.GetFloat(MIXER_MUSIC, out volume);
        audioSlider_Music.value = volume;

        optionsSO.MainAudioMixer.GetFloat(MIXER_SFX, out volume);
        audioSlider_SFX.value = volume;

        optionsSO.MainAudioMixer.GetFloat(MIXER_AMBIENCE, out volume);
        audioSlider_Ambience.value = volume;

        checkbox_ControllerRumble.isOn = optionsSO.ControllerRumble;
        checkbox_Speedrun.isOn = false;
        optionsSO.SetSpeedRunMode(false);

        switch (optionsSO.Difficulty)
        {
            case Difficulty.Easy:
                toggle_EasyDifficulty.isOn = true;
                break;
            case Difficulty.Medium:
                toggle_MediumDifficulty.isOn = true;
                break;
            case Difficulty.Hard:
                toggle_HardDifficulty.isOn = true;
                break;
        }
    }

    private void OnEnable()
    {
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        controls.Cancel.performed += Cancel;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        onOptionsClose?.Invoke();
    }

    private void OnDisable()
    {
        controls.Cancel.performed -= Cancel;
        controls.Disable();
    }
}
