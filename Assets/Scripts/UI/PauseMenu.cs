using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReaderSO inputReader;

    [Header("Inspector Objects")]
    [SerializeField] private Button firstSelectButton;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO gamePausedEventSO;
    [SerializeField] private VoidEventSO gameUnpausedEventSO;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO gameQuitEventSO;
    [SerializeField] private VoidEventSO activatePauseAbilitySO;

    // Properties
    private PlayerControls.GameplayControlsActions Controls => inputReader.Controls;

    // Private Variables
    private bool isPaused;
    private bool hasAbilityToPause;

    private void OnEnable()
    {
        Controls.Pause.performed += Pause;
        gameQuitEventSO.OnEventRaised += ResetPauseMenu;

        if (activatePauseAbilitySO != null) activatePauseAbilitySO.OnEventRaised += ActivateAbilityToPause;
        else hasAbilityToPause = true;
    }

    private void OnDisable()
    {
        Controls.Pause.performed -= Pause;
        gameQuitEventSO.OnEventRaised -= ResetPauseMenu;

        if (activatePauseAbilitySO != null) activatePauseAbilitySO.OnEventRaised -= ActivateAbilityToPause;
        else hasAbilityToPause = false;
    }

    private void ActivateAbilityToPause()
    {
        hasAbilityToPause = true;
    }

    private void Pause(InputAction.CallbackContext _)
    {
        if (!hasAbilityToPause) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;

        pauseMenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectButton.gameObject);
            InputSystem.PauseHaptics();
            gamePausedEventSO.RaiseEvent();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            InputSystem.ResumeHaptics();
            gameUnpausedEventSO.RaiseEvent();
        }
    }

    public void Pause()
    {
        if (!hasAbilityToPause) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;

        pauseMenuPanel.SetActive(isPaused);

        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectButton.gameObject);
            InputSystem.PauseHaptics();
            gamePausedEventSO.RaiseEvent();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            InputSystem.ResumeHaptics();
            gameUnpausedEventSO.RaiseEvent();
        }
    }

    private void ResetPauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1;
    }
}
